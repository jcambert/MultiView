using System.Collections.Generic;

namespace MultiView.DynamicViews.Domain.Model;

public sealed class ViewActionDefinition
{
    public required string Name { get; init; }

    public required string Label { get; init; }

    public string? Icon { get; init; }

    public string? CssClass { get; init; }

    public string? VisibilityRule { get; init; }

    public string? EnabledRule { get; init; }
}