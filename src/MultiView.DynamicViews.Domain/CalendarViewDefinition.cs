namespace MultiView.DynamicViews.Domain.Model;

public sealed class CalendarViewDefinition : DynamicViewDefinition
{
    public required string StartDateField { get; init; }

    public string? EndDateField { get; init; }

    public string? TitleField { get; init; }

    public string? SubtitleField { get; init; }

    public string Bucket { get; init; } = "day";

    public int? LimitPerBucket { get; init; }
}
