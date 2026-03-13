namespace MultiView.DynamicViews.Domain.Model;

public sealed class KanbanColumnDefinition
{
    public required string Key { get; init; }

    public required string Title { get; init; }

    public string? Value { get; init; }

    public string? When { get; init; }

    public int Order { get; init; } = int.MaxValue;
}