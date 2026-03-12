namespace MultiView.DynamicViews.Domain.Model;

public sealed class GanttViewDefinition : DynamicViewDefinition
{
    public required string StartDateField { get; init; }

    public required string EndDateField { get; init; }

    public required string LabelField { get; init; }

    public string? GroupByField { get; init; }

    public string? ProgressField { get; init; }

    public int? Limit { get; init; }
}
