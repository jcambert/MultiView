namespace MultiView.DynamicViews.Domain.Model;

public sealed class ViewRuleSet
{
    public string? Visible { get; init; }

    public string? Readonly { get; init; }

    public string? Required { get; init; }

    public string? Enabled { get; init; }
}