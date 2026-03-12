namespace MultiView.DynamicViews.Domain.Model;

public sealed class ViewCompositionDefinition
{
    public IReadOnlyList<string> Includes { get; init; } = [];

    public IReadOnlyList<string> RemoveFields { get; init; } = [];

    public IReadOnlyList<string> RemoveSections { get; init; } = [];

    public IReadOnlyList<string> RemoveColumns { get; init; } = [];

    public IReadOnlyList<string> RemoveActions { get; init; } = [];
}
