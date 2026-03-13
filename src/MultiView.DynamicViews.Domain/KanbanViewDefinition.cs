namespace MultiView.DynamicViews.Domain.Model;

public sealed class KanbanViewDefinition : DynamicViewDefinition
{
    public string? GroupByField { get; init; }

    public IReadOnlyList<KanbanColumnDefinition> Columns { get; init; } = [];

    public bool? ShowUnassignedColumn { get; init; }

    public required KanbanCardDefinition Card { get; init; }
}
