namespace MultiView.DynamicViews.Domain.Model;

public sealed class SearchViewDefinition : DynamicViewDefinition
{
    public bool EnablePaging { get; init; } = true;

    public IReadOnlyList<string> SearchFields { get; init; } = [];

    public int? DefaultPageSize { get; init; }
}
