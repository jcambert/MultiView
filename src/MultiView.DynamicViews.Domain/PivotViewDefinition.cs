namespace MultiView.DynamicViews.Domain.Model;

public sealed class PivotViewDefinition : DynamicViewDefinition
{
    public required string RowField { get; init; }

    public required string ColumnField { get; init; }

    public required string ValueField { get; init; }

    public string Aggregation { get; init; } = "Sum";

    public int? ValuePrecision { get; init; }
}
