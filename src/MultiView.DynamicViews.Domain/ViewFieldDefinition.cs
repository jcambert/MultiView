using System.Text.Json;

namespace MultiView.DynamicViews.Domain.Model;

public sealed class ViewFieldDefinition
{
    public required string Name { get; init; }

    public string? Label { get; init; }

    public required ViewFieldKind Kind { get; init; }

    public string? Widget { get; init; }

    public string? SearchWidget { get; init; }

    public string? SearchLabel { get; init; }

    public JsonElement? SearchWidgetOptions { get; init; }

    public string? Format { get; init; }

    public string? CssClass { get; init; }

    public JsonElement? DefaultValue { get; init; }

    public JsonElement? WidgetOptions { get; init; }

    public ViewRuleSet? Rules { get; init; }
}
