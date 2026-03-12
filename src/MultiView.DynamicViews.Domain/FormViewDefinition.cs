namespace MultiView.DynamicViews.Domain.Model;

public sealed class FormViewDefinition : DynamicViewDefinition
{
    public IReadOnlyList<FormSectionDefinition> Sections { get; init; } = [];
}