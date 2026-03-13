namespace MultiView.DynamicViews.Domain.Model;

public sealed class ListViewDefinition : DynamicViewDefinition
{
    public IReadOnlyList<ListColumnDefinition> Columns { get; init; } = [];

    public bool EnableSearch { get; init; } = true;

    public bool EnablePaging { get; init; } = true;

    public int? DefaultPageSize { get; init; }
}
