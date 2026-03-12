using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Core.Widgets;

public interface IFieldWidgetRegistry
{
    void Register(string widgetKey, Type componentType);

    void RegisterFallback(ViewFieldKind kind, Type componentType);

    Type? Resolve(ViewFieldDefinition fieldDefinition);
}
