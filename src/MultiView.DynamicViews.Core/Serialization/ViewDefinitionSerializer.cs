using MultiView.DynamicViews.Domain.Model;
using MultiView.DynamicViews.Core.Abstractions;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace MultiView.DynamicViews.Core.Serialization;

public sealed class ViewDefinitionSerializer : IViewDefinitionSerializer
{
    private readonly JsonSerializerOptions _options;

    public ViewDefinitionSerializer()
    {
        _options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _options.Converters.Add(new JsonStringEnumConverter());
    }

    public DynamicViewDefinition Deserialize(string json)
    {
        using JsonDocument document = JsonDocument.Parse(json);
        if (!document.RootElement.TryGetProperty("kind", out JsonElement kindElement)
            && !document.RootElement.TryGetProperty("Kind", out kindElement))
        {
            throw new InvalidOperationException("Le champ 'kind' est obligatoire dans la définition de vue.");
        }

        string? kindValue = kindElement.GetString();
        if (string.IsNullOrWhiteSpace(kindValue))
        {
            throw new InvalidOperationException("Le champ 'kind' doit être renseigné.");
        }

        DynamicViewKind kind = ParseKind(kindValue!);

        return kind switch
        {
            DynamicViewKind.Form => JsonSerializer.Deserialize<FormViewDefinition>(json, _options)!,
            DynamicViewKind.List => JsonSerializer.Deserialize<ListViewDefinition>(json, _options)!,
            DynamicViewKind.Kanban => JsonSerializer.Deserialize<KanbanViewDefinition>(json, _options)!,
            DynamicViewKind.Search => JsonSerializer.Deserialize<SearchViewDefinition>(json, _options)!,
            _ => throw new NotSupportedException($"Type de vue non supporté: {kind}")
        };
    }

    public string Serialize(DynamicViewDefinition definition)
    {
        return JsonSerializer.Serialize(definition, definition.GetType(), _options);
    }

    private static DynamicViewKind ParseKind(string rawKind)
    {
        if (Enum.TryParse<DynamicViewKind>(rawKind, ignoreCase: true, out DynamicViewKind kind))
        {
            return kind;
        }

        throw new InvalidOperationException($"Type de vue inconnu: {rawKind}.");
    }
}
