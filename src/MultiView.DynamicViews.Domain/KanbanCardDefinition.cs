using System.Collections.Generic;

namespace MultiView.DynamicViews.Domain.Model;

public sealed class KanbanCardDefinition
{
    public required string HeaderField { get; init; }

    public required string FooterField { get; init; }

    public IReadOnlyList<string> DetailFields { get; init; } = [];

    public string? ColorField { get; init; }
}