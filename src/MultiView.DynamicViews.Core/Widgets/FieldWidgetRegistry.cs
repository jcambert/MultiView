using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Core.Widgets;

public sealed class FieldWidgetRegistry : IFieldWidgetRegistry
{
    private readonly object _sync = new();
    private readonly Dictionary<string, Type> _widgets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<ViewFieldKind, Type> _fallbackByKind = new();
    private readonly Dictionary<(string WidgetKey, ViewFieldKind Kind), Type?> _resolvedCache = new();

    public void Register(string widgetKey, Type componentType)
    {
        if (string.IsNullOrWhiteSpace(widgetKey))
        {
            throw new ArgumentException("Le nom du widget est requis.", nameof(widgetKey));
        }

        lock (_sync)
        {
            _widgets[widgetKey.Trim()] = componentType;
            _resolvedCache.Clear();
        }
    }

    public void RegisterFallback(ViewFieldKind kind, Type componentType)
    {
        lock (_sync)
        {
            _fallbackByKind[kind] = componentType;
            _resolvedCache.Clear();
        }
    }

    public Type? Resolve(ViewFieldDefinition fieldDefinition)
    {
        lock (_sync)
        {
            string widgetKey = fieldDefinition.Widget ?? fieldDefinition.Kind.ToString();
            var cacheKey = (WidgetKey: widgetKey, Kind: fieldDefinition.Kind);
            if (_resolvedCache.TryGetValue(cacheKey, out Type? cached))
            {
                return cached;
            }

            Type? resolved = null;
            if (_widgets.TryGetValue(widgetKey, out Type? byWidget))
            {
                resolved = byWidget;
            }
            else if (_fallbackByKind.TryGetValue(fieldDefinition.Kind, out Type? byKind))
            {
                resolved = byKind;
            }

            _resolvedCache[cacheKey] = resolved;
            return resolved;
        }
    }
}
