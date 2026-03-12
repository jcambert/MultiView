namespace MultiView.DynamicViews.Domain.Model;

public sealed class ListColumnDefinition
{
    public required string Field { get; init; }

    public string? Header { get; init; }

    public string? Align { get; init; }

    public bool Sortable { get; init; } = true;

    public int? Width { get; init; }
}