using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Blazor.Components.Core;

public sealed record FieldRenderContext(
    object Record,
    ViewFieldDefinition Field,
    bool IsVisible,
    bool IsEnabled,
    bool IsReadOnly,
    bool IsRequired,
    IReadOnlyDictionary<string, object?>? GlobalContext);
