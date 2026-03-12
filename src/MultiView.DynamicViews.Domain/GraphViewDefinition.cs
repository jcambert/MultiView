namespace MultiView.DynamicViews.Domain.Model;

public sealed class GraphViewDefinition : DynamicViewDefinition
{
    public required string CategoryField { get; init; }

    public required string ValueField { get; init; }

    public string? SeriesField { get; init; }

    public string Aggregation { get; init; } = "Sum";

    public string ChartType { get; init; } = "Bar";

    public int? Limit { get; init; }
}
