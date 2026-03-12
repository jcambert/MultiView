using System.Collections.Generic;

namespace MultiView.DynamicViews.Domain.Model;

public sealed class FormSectionDefinition
{
    public required string Id { get; init; }

    public string? Label { get; init; }

    public IReadOnlyList<string> Fields { get; init; } = [];

    public int Columns { get; init; } = 1;
}