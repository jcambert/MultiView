namespace MultiView.DynamicViews.Domain.Model;

public abstract class DynamicViewDefinition
{
    public required string Id { get; init; }

    public required string Model { get; init; }

    public required string Name { get; init; }

    public required DynamicViewKind Kind { get; init; }

    public string? Title { get; init; }

    public string? Extends { get; init; }

    public ViewCompositionDefinition? Composition { get; init; }

    public IReadOnlyList<ViewFieldDefinition> Fields { get; init; } = [];

    public IReadOnlyList<ViewActionDefinition> Actions { get; init; } = [];

    public ViewRuleSet? Rules { get; init; }
}
