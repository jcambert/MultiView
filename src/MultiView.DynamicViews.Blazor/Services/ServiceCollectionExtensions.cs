using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MudBlazor.Services;
using MultiView.DynamicViews.Core;
using MultiView.DynamicViews.Core.Widgets;
using MultiView.DynamicViews.Domain.Model;
using MultiView.DynamicViews.Blazor.Components.Widgets;

namespace MultiView.DynamicViews.Blazor;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDynamicViewsMudBlazor(
        this IServiceCollection services,
        Action<IFieldWidgetRegistry>? configureWidgets = null)
    {
        services.AddMudServices();
        services.AddDynamicViewsCore();

        services.RemoveAll<IFieldWidgetRegistry>();
        FieldWidgetRegistry registry = new();
        RegisterBuiltInWidgets(registry);
        configureWidgets?.Invoke(registry);
        services.AddSingleton<IFieldWidgetRegistry>(_ => registry);

        return services;
    }

    private static void RegisterBuiltInWidgets(IFieldWidgetRegistry registry)
    {
        registry.RegisterFallback(ViewFieldKind.Text, typeof(TextFieldWidget));
        registry.RegisterFallback(ViewFieldKind.Email, typeof(TextFieldWidget));
        registry.RegisterFallback(ViewFieldKind.Phone, typeof(TextFieldWidget));
        registry.RegisterFallback(ViewFieldKind.Url, typeof(TextFieldWidget));

        registry.RegisterFallback(ViewFieldKind.Number, typeof(NumberFieldWidget));
        registry.RegisterFallback(ViewFieldKind.Decimal, typeof(CurrencyFieldWidget));
        registry.Register(ViewFieldKind.Date.ToString(), typeof(DateFieldWidget));
        registry.Register(ViewFieldKind.Boolean.ToString(), typeof(BooleanFieldWidget));
        registry.Register(ViewFieldKind.Select.ToString(), typeof(SelectFieldWidget));
        registry.Register(ViewFieldKind.Currency.ToString(), typeof(CurrencyFieldWidget));

        registry.Register("text", typeof(TextFieldWidget));
        registry.Register("number", typeof(NumberFieldWidget));
        registry.Register("decimal", typeof(CurrencyFieldWidget));
        registry.Register("currency", typeof(CurrencyFieldWidget));
        registry.Register("date", typeof(DateFieldWidget));
        registry.Register("boolean", typeof(BooleanFieldWidget));
        registry.Register("select", typeof(SelectFieldWidget));
    }
}