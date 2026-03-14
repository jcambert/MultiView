using System.Text.Json;
using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Core.Validation;

public sealed class JsonViewDefinitionValidator : IViewDefinitionValidator
{
    private static readonly HashSet<string> CommonRootProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "id", "model", "name", "kind", "title", "extends", "composition", "fields", "actions", "rules"
    };

    private static readonly Dictionary<DynamicViewKind, HashSet<string>> KindRootProperties = new()
    {
        [DynamicViewKind.Form] = ["sections"],
        [DynamicViewKind.List] = ["columns", "enableSearch", "enablePaging", "defaultPageSize"],
        [DynamicViewKind.Kanban] = ["groupByField", "columns", "showUnassignedColumn", "card"],
        [DynamicViewKind.Search] = ["searchFields", "enablePaging", "defaultPageSize"],
        [DynamicViewKind.Graph] = ["categoryField", "valueField", "seriesField", "aggregation", "chartType", "limit"],
        [DynamicViewKind.Pivot] = ["rowField", "columnField", "valueField", "aggregation", "valuePrecision"],
        [DynamicViewKind.Calendar] = ["startDateField", "endDateField", "titleField", "subtitleField", "bucket", "limitPerBucket"],
        [DynamicViewKind.Gantt] = ["startDateField", "endDateField", "labelField", "groupByField", "progressField", "limit"]
    };

    private static readonly HashSet<string> FieldProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "name", "label", "kind", "widget", "searchWidget", "searchLabel", "searchWidgetOptions", "format", "cssClass", "defaultValue", "widgetOptions", "rules"
    };

    private static readonly HashSet<string> ActionProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "name", "label", "icon", "cssClass", "visibilityRule", "enabledRule"
    };

    private readonly ViewDefinitionValidationOptions _options;

    public JsonViewDefinitionValidator(ViewDefinitionValidationOptions options) => _options = options;

    public ViewDefinitionValidationResult Validate(string json)
    {
        ViewDefinitionValidationResult result = new();
        if (string.IsNullOrWhiteSpace(json))
        {
            result.AddError("$", "Le JSON de definition est vide.");
            return result;
        }

        JsonDocument? document = null;
        try { document = JsonDocument.Parse(json); }
        catch (JsonException exception)
        {
            result.AddError("$", $"JSON invalide (ligne {exception.LineNumber}, position {exception.BytePositionInLine}): {exception.Message}");
            return result;
        }

        using (document)
        {
            JsonElement root = document.RootElement;
            if (root.ValueKind != JsonValueKind.Object)
            {
                result.AddError("$", "La racine JSON doit etre un objet.");
                return result;
            }

            RequiredString(root, "id", "$.id", result);
            RequiredString(root, "model", "$.model", result);
            RequiredString(root, "name", "$.name", result);
            RequiredString(root, "kind", "$.kind", result);
            ValidateFields(root, result);
            ValidateActions(root, result);

            HashSet<string> fieldNames = CollectFieldNames(root, result);
            DynamicViewKind? kind = ParseKind(root, result);
            if (!kind.HasValue)
            {
                return result;
            }

            HashSet<string> allowedRoot = new(CommonRootProperties, StringComparer.OrdinalIgnoreCase);
            foreach (string property in KindRootProperties[kind.Value]) { allowedRoot.Add(property); }
            ValidateUnknown(root, allowedRoot, "$", result);

            ValidateReferences(root, kind.Value, fieldNames, result);
        }

        return result;
    }

    private void ValidateFields(JsonElement root, ViewDefinitionValidationResult result)
    {
        if (!TryGet(root, "fields", out JsonElement fields)) { return; }
        if (fields.ValueKind != JsonValueKind.Array)
        {
            result.AddError("$.fields", "La propriete 'fields' doit etre un tableau.");
            return;
        }

        int index = 0;
        foreach (JsonElement field in fields.EnumerateArray())
        {
            string path = $"$.fields[{index}]";
            if (field.ValueKind != JsonValueKind.Object)
            {
                result.AddError(path, "Chaque champ doit etre un objet.");
                index++;
                continue;
            }

            RequiredString(field, "name", $"{path}.name", result);
            RequiredString(field, "kind", $"{path}.kind", result);
            ValidateUnknown(field, FieldProperties, path, result);

            if (TryGet(field, "kind", out JsonElement kindElement) && kindElement.ValueKind == JsonValueKind.String)
            {
                string? kindRaw = kindElement.GetString();
                if (!string.IsNullOrWhiteSpace(kindRaw) && !Enum.TryParse<ViewFieldKind>(kindRaw, true, out _))
                {
                    result.AddError($"{path}.kind", $"Kind de champ inconnu: '{kindRaw}'.");
                }
            }

            index++;
        }
    }

    private void ValidateActions(JsonElement root, ViewDefinitionValidationResult result)
    {
        if (!TryGet(root, "actions", out JsonElement actions)) { return; }
        if (actions.ValueKind != JsonValueKind.Array)
        {
            result.AddError("$.actions", "La propriete 'actions' doit etre un tableau.");
            return;
        }

        int index = 0;
        foreach (JsonElement action in actions.EnumerateArray())
        {
            string path = $"$.actions[{index}]";
            if (action.ValueKind != JsonValueKind.Object)
            {
                result.AddError(path, "Chaque action doit etre un objet.");
                index++;
                continue;
            }

            RequiredString(action, "name", $"{path}.name", result);
            RequiredString(action, "label", $"{path}.label", result);
            ValidateUnknown(action, ActionProperties, path, result);
            index++;
        }
    }

    private static HashSet<string> CollectFieldNames(JsonElement root, ViewDefinitionValidationResult result)
    {
        HashSet<string> names = new(StringComparer.OrdinalIgnoreCase);
        if (!TryGet(root, "fields", out JsonElement fields) || fields.ValueKind != JsonValueKind.Array) { return names; }

        int index = 0;
        foreach (JsonElement field in fields.EnumerateArray())
        {
            if (TryGet(field, "name", out JsonElement nameElement) && nameElement.ValueKind == JsonValueKind.String)
            {
                string? name = nameElement.GetString();
                if (!string.IsNullOrWhiteSpace(name) && !names.Add(name))
                {
                    result.AddError($"$.fields[{index}].name", $"Le champ '{name}' est duplique.");
                }
            }
            index++;
        }

        return names;
    }

    private void ValidateReferences(JsonElement root, DynamicViewKind kind, IReadOnlySet<string> fields, ViewDefinitionValidationResult result)
    {
        switch (kind)
        {
            case DynamicViewKind.Form: RefArray(root, "sections", "fields", "$.sections", fields, result); break;
            case DynamicViewKind.List: RefArray(root, "columns", "field", "$.columns", fields, result); IntMin(root, "defaultPageSize", "$.defaultPageSize", 1, result); break;
            case DynamicViewKind.Kanban:
                Ref(root, "groupByField", "$.groupByField", fields, result);
                RequiredObject(root, "card", "$.card", result);
                if (TryGet(root, "card", out JsonElement card) && card.ValueKind == JsonValueKind.Object)
                {
                    RequiredString(card, "headerField", "$.card.headerField", result); Ref(card, "headerField", "$.card.headerField", fields, result);
                    RequiredString(card, "footerField", "$.card.footerField", result); Ref(card, "footerField", "$.card.footerField", fields, result);
                    RefArrayItems(card, "detailFields", "$.card.detailFields", fields, result);
                    Ref(card, "colorField", "$.card.colorField", fields, result);
                }
                break;
            case DynamicViewKind.Search: RefArrayItems(root, "searchFields", "$.searchFields", fields, result); IntMin(root, "defaultPageSize", "$.defaultPageSize", 1, result); break;
            case DynamicViewKind.Graph: RequiredString(root, "categoryField", "$.categoryField", result); RequiredString(root, "valueField", "$.valueField", result); Ref(root, "categoryField", "$.categoryField", fields, result); Ref(root, "valueField", "$.valueField", fields, result); Ref(root, "seriesField", "$.seriesField", fields, result); break;
            case DynamicViewKind.Pivot: RequiredString(root, "rowField", "$.rowField", result); RequiredString(root, "columnField", "$.columnField", result); RequiredString(root, "valueField", "$.valueField", result); Ref(root, "rowField", "$.rowField", fields, result); Ref(root, "columnField", "$.columnField", fields, result); Ref(root, "valueField", "$.valueField", fields, result); break;
            case DynamicViewKind.Calendar: RequiredString(root, "startDateField", "$.startDateField", result); Ref(root, "startDateField", "$.startDateField", fields, result); Ref(root, "endDateField", "$.endDateField", fields, result); Ref(root, "titleField", "$.titleField", fields, result); Ref(root, "subtitleField", "$.subtitleField", fields, result); break;
            case DynamicViewKind.Gantt: RequiredString(root, "startDateField", "$.startDateField", result); RequiredString(root, "endDateField", "$.endDateField", result); RequiredString(root, "labelField", "$.labelField", result); Ref(root, "startDateField", "$.startDateField", fields, result); Ref(root, "endDateField", "$.endDateField", fields, result); Ref(root, "labelField", "$.labelField", fields, result); Ref(root, "groupByField", "$.groupByField", fields, result); Ref(root, "progressField", "$.progressField", fields, result); break;
        }
    }

    private static void Ref(JsonElement parent, string name, string path, IReadOnlySet<string> fields, ViewDefinitionValidationResult result)
    {
        if (!TryGet(parent, name, out JsonElement value) || value.ValueKind != JsonValueKind.String) { return; }
        string? field = value.GetString();
        if (!string.IsNullOrWhiteSpace(field) && !fields.Contains(field)) { result.AddError(path, $"Reference de champ inconnue: '{field}'."); }
    }

    private static void RefArrayItems(JsonElement parent, string name, string path, IReadOnlySet<string> fields, ViewDefinitionValidationResult result)
    {
        if (!TryGet(parent, name, out JsonElement value)) { return; }
        if (value.ValueKind != JsonValueKind.Array) { result.AddError(path, "Ce champ doit etre un tableau."); return; }

        int index = 0;
        foreach (JsonElement item in value.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.String) { result.AddError($"{path}[{index}]", "Chaque element doit etre une chaine."); index++; continue; }
            string? field = item.GetString();
            if (!string.IsNullOrWhiteSpace(field) && !fields.Contains(field)) { result.AddError($"{path}[{index}]", $"Reference de champ inconnue: '{field}'."); }
            index++;
        }
    }

    private static void RefArray(JsonElement root, string collectionName, string propertyName, string path, IReadOnlySet<string> fields, ViewDefinitionValidationResult result)
    {
        if (!TryGet(root, collectionName, out JsonElement collection)) { return; }
        if (collection.ValueKind != JsonValueKind.Array) { result.AddError(path, "Ce champ doit etre un tableau."); return; }

        int index = 0;
        foreach (JsonElement item in collection.EnumerateArray())
        {
            if (item.ValueKind != JsonValueKind.Object) { result.AddError($"{path}[{index}]", "Chaque element doit etre un objet."); index++; continue; }
            Ref(item, propertyName, $"{path}[{index}].{propertyName}", fields, result);
            index++;
        }
    }

    private static void IntMin(JsonElement parent, string name, string path, int min, ViewDefinitionValidationResult result)
    {
        if (!TryGet(parent, name, out JsonElement value) || value.ValueKind == JsonValueKind.Null) { return; }
        if (value.ValueKind != JsonValueKind.Number || !value.TryGetInt32(out int intValue)) { result.AddError(path, "Ce champ doit etre un entier."); return; }
        if (intValue < min) { result.AddError(path, $"La valeur doit etre >= {min}."); }
    }

    private void ValidateUnknown(JsonElement element, IReadOnlySet<string> allowed, string path, ViewDefinitionValidationResult result)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (!allowed.Contains(property.Name))
            {
                switch (_options.UnknownFieldHandling)
                {
                    case UnknownJsonFieldHandling.Warning: result.AddWarning($"{path}.{property.Name}", $"Champ inconnu '{property.Name}'."); break;
                    case UnknownJsonFieldHandling.Error: result.AddError($"{path}.{property.Name}", $"Champ inconnu '{property.Name}'."); break;
                }
            }
        }
    }

    private static DynamicViewKind? ParseKind(JsonElement root, ViewDefinitionValidationResult result)
    {
        if (!TryGet(root, "kind", out JsonElement kindElement)) { return null; }
        if (kindElement.ValueKind != JsonValueKind.String) { result.AddError("$.kind", "Le champ 'kind' doit etre une chaine."); return null; }
        string? raw = kindElement.GetString();
        if (string.IsNullOrWhiteSpace(raw)) { result.AddError("$.kind", "Le champ 'kind' doit etre renseigne."); return null; }
        if (!Enum.TryParse<DynamicViewKind>(raw, true, out DynamicViewKind kind)) { result.AddError("$.kind", $"Type de vue inconnu: '{raw}'."); return null; }
        return kind;
    }

    private static void RequiredString(JsonElement parent, string name, string path, ViewDefinitionValidationResult result)
    {
        if (!TryGet(parent, name, out JsonElement value)) { result.AddError(path, "Champ obligatoire manquant."); return; }
        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString())) { result.AddError(path, "Ce champ doit etre une chaine non vide."); }
    }

    private static void RequiredObject(JsonElement parent, string name, string path, ViewDefinitionValidationResult result)
    {
        if (!TryGet(parent, name, out JsonElement value)) { result.AddError(path, "Champ obligatoire manquant."); return; }
        if (value.ValueKind != JsonValueKind.Object) { result.AddError(path, "Ce champ doit etre un objet."); }
    }

    private static bool TryGet(JsonElement element, string propertyName, out JsonElement value)
    {
        foreach (JsonProperty property in element.EnumerateObject())
        {
            if (string.Equals(property.Name, propertyName, StringComparison.OrdinalIgnoreCase))
            {
                value = property.Value;
                return true;
            }
        }

        value = default;
        return false;
    }
}
