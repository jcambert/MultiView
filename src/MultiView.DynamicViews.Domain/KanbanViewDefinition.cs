namespace MultiView.DynamicViews.Domain.Model;

public sealed class KanbanViewDefinition : DynamicViewDefinition
{
    public required string GroupByField { get; init; }

    public required KanbanCardDefinition Card { get; init; }
}