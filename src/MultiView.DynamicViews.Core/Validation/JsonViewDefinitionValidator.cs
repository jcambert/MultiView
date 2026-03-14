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
        [DynamicViewKind.Form] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "sections" },
        [DynamicViewKind.List] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "columns", "enableSearch", "enablePaging", "defaultPageSize" },
        [DynamicViewKind.Kanban] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "groupByField", "columns", "showUnassignedColumn", "card" },
        [DynamicViewKind.Search] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "searchFields", "enablePaging", "defaultPageSize" },
        [DynamicViewKind.Graph] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "categoryField", "valueField", "seriesField", "aggregation", "chartType", "limit" },
        [DynamicViewKind.Pivot] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "rowField", "columnField", "valueField", "aggregation", "valuePrecision" },
        [DynamicViewKind.Calendar] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "startDateField", "endDateField", "titleField", "subtitleField", "bucket", "limitPerBucket" },
        [DynamicViewKind.Gantt] = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { "startDateField", "endDateField", "labelField", "groupByField", "progressField", "limit" }
    };

    private static readonly HashSet<string> FieldProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        "name", "label", "kind", "widget", "searchWidget", "searchLabel", "searchWidgetOptions", "format", "cssClass", "defaultValue", "widgetOptions", "rules"
    };

    private readonly ViewDefinitionValidationOptions _options;

    public JsonViewDefinitionValidator(ViewDefinitionValidationOptions options)
    {
        _options = options;
    }

    public ViewDefinitionValidationResult Validate(string json)
    {
        ViewDefinitionValidationResult result = new();

        if (string.IsNullOrWhiteSpace(json))
        {
            result.AddError("$", "Le JSON de definition est vide.");
            return result;
        }

        JsonDocument? document = null;
        try
        {
            document = JsonDocument.Parse(json);
        }
        catch (JsonException exception)
        {
            result.AddError(
                "$",
                $"JSON invalide (ligne {exception.LineNumber}, position {exception.BytePositionInLine}): {exception.Message}");
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

            ValidateRequiredString(root, "id", "$.id", result);
            ValidateRequiredString(root, "model", "$.model", result);
            ValidateRequiredString(root, "name", "$.name", result);
            ValidateRequiredString(root, "kind", "$.kind", result);
            ValidateFields(root, result);

            DynamicViewKind? kind = ParseKind(root, result);
            if (kind.HasValue)
            {
                ValidateUnknownRootProperties(root, kind.Value, result);
                ValidateKindSpecific(root, kind.Value, result);
            }
        }

        return result;
    }

    private void ValidateKindSpecific(JsonElement root, DynamicViewKind kind, ViewDefinitionValidationResult result)
    {
        switch (kind)
        {
            case DynamicViewKind.Kanban:
                ValidateRequiredObject(root, "card", "$.card", result);
                break;
            case DynamicViewKind.Graph:
                ValidateRequiredString(root, "categoryField", "$.categoryField", result);
                ValidateRequiredString(root, "valueField", "$.valueField", result);
                break;
            case DynamicViewKind.Pivot:
                ValidateRequiredString(root, "rowField", "$.rowField", result);
                ValidateRequiredString(root, "columnField", "$.columnField", result);
                ValidateRequiredString(root, "valueField", "$.valueField", result);
                break;
            case DynamicViewKind.Calendar:
                ValidateRequiredString(root, "startDateField", "$.startDateField", result);
                break;
            case DynamicViewKind.Gantt:
                ValidateRequiredString(root, "startDateField", "$.startDateField", result);
                ValidateRequiredString(root, "endDateField", "$.endDateField", result);
                ValidateRequiredString(root, "labelField", "$.labelField", result);
                break;
        }
    }

    private void ValidateUnknownRootProperties(JsonElement root, DynamicViewKind kind, ViewDefinitionValidationResult result)
    {
        HashSet<string> allowedProperties = new(CommonRootProperties, StringComparer.OrdinalIgnoreCase);
        foreach (string propertyName in KindRootProperties[kind])
        {
            allowedProperties.Add(propertyName);
        }

        foreach (JsonProperty property in root.EnumerateObject())
        {
            if (allowedProperties.Contains(property.Name))
            {
                continue;
            }

            HandleUnknownProperty($"$.{property.Name}", $"Champ inconnu '{property.Name}'.", result);
        }
    }

    private void ValidateFields(JsonElement root, ViewDefinitionValidationResult result)
    {
        if (!TryGetPropertyIgnoreCase(root, "fields", out JsonElement fields))
        {
            return;
        }

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

            ValidateRequiredString(field, "name", $"{path}.name", result);
            ValidateRequiredString(field, "kind", $"{path}.kind", result);

            foreach (JsonProperty property in field.EnumerateObject())
            {
                if (FieldProperties.Contains(property.Name))
                {
                    continue;
                }

                HandleUnknownProperty($"{path}.{property.Name}", $"Champ inconnu '{property.Name}'.", result);
            }

            if (TryGetPropertyIgnoreCase(field, "kind", out JsonElement kindElement)
                && kindElement.ValueKind == JsonValueKind.String)
            {
                string? kindRaw = kindElement.GetString();
                if (!string.IsNullOrWhiteSpace(kindRaw)
                    && !Enum.TryParse<ViewFieldKind>(kindRaw, ignoreCase: true, out _))
                {
                    result.AddError($"{path}.kind", $"Kind de champ inconnu: '{kindRaw}'.");
                }
            }

            index++;
        }
    }

    private void HandleUnknownProperty(string path, string message, ViewDefinitionValidationResult result)
    {
        switch (_options.UnknownFieldHandling)
        {
            case UnknownJsonFieldHandling.Ignore:
                break;
            case UnknownJsonFieldHandling.Warning:
                result.AddWarning(path, message);
                break;
            case UnknownJsonFieldHandling.Error:
            default:
                result.AddError(path, message);
                break;
        }
    }

    private static DynamicViewKind? ParseKind(JsonElement root, ViewDefinitionValidationResult result)
    {
        if (!TryGetPropertyIgnoreCase(root, "kind", out JsonElement kindElement))
        {
            return null;
        }

        if (kindElement.ValueKind != JsonValueKind.String)
        {
            result.AddError("$.kind", "Le champ 'kind' doit etre une chaine.");
            return null;
        }

        string? kindRawValue = kindElement.GetString();
        if (string.IsNullOrWhiteSpace(kindRawValue))
        {
            result.AddError("$.kind", "Le champ 'kind' doit etre renseigne.");
            return null;
        }

        if (!Enum.TryParse<DynamicViewKind>(kindRawValue, ignoreCase: true, out DynamicViewKind parsedKind))
        {
            result.AddError("$.kind", $"Type de vue inconnu: '{kindRawValue}'.");
            return null;
        }

        return parsedKind;
    }

    private static void ValidateRequiredString(JsonElement parent, string propertyName, string path, ViewDefinitionValidationResult result)
    {
        if (!TryGetPropertyIgnoreCase(parent, propertyName, out JsonElement value))
        {
            result.AddError(path, "Champ obligatoire manquant.");
            return;
        }

        if (value.ValueKind != JsonValueKind.String || string.IsNullOrWhiteSpace(value.GetString()))
        {
            result.AddError(path, "Ce champ doit etre une chaine non vide.");
        }
    }

    private static void ValidateRequiredObject(JsonElement parent, string propertyName, string path, ViewDefinitionValidationResult result)
    {
        if (!TryGetPropertyIgnoreCase(parent, propertyName, out JsonElement value))
        {
            result.AddError(path, "Champ obligatoire manquant.");
            return;
        }

        if (value.ValueKind != JsonValueKind.Object)
        {
            result.AddError(path, "Ce champ doit etre un objet.");
        }
    }

    private static bool TryGetPropertyIgnoreCase(JsonElement element, string propertyName, out JsonElement value)
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
