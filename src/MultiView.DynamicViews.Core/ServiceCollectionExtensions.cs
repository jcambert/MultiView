using Microsoft.Extensions.DependencyInjection;
using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Core.Caching;
using MultiView.DynamicViews.Core.RuleEvaluator;
using MultiView.DynamicViews.Core.Services;
using MultiView.DynamicViews.Core.Widgets;
using MultiView.DynamicViews.Core.Serialization;
using MultiView.DynamicViews.Core.Validation;
using MultiView.DynamicViews.Core.Views;

namespace MultiView.DynamicViews.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDynamicViewsCore(this IServiceCollection services)
    {
        services.AddMemoryCache();
        services.AddSingleton<ViewDefinitionValidationOptions>();
        services.AddSingleton<IViewDefinitionValidator, JsonViewDefinitionValidator>();
        services.AddSingleton<IRuleUserContextAccessor, EmptyRuleUserContextAccessor>();
        services.AddSingleton<IRecordPropertyAccessor, ReflectionRecordPropertyAccessor>();
        services.AddSingleton<IViewDefinitionSerializer, ViewDefinitionSerializer>();
        services.AddSingleton<IRuleEvaluator, DefaultRuleEvaluator>();
        services.AddSingleton<IFieldWidgetRegistry, FieldWidgetRegistry>();
        services.AddSingleton<IViewActionDispatcher, DefaultViewActionDispatcher>();

        return services;
    }

    public static IServiceCollection ConfigureDynamicViewValidation(
        this IServiceCollection services,
        Action<ViewDefinitionValidationOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(configure);

        services.AddSingleton(serviceProvider =>
        {
            ViewDefinitionValidationOptions options = serviceProvider.GetService<ViewDefinitionValidationOptions>()
                ?? new ViewDefinitionValidationOptions();
            configure(options);
            return options;
        });

        return services;
    }

    public static IServiceCollection AddDynamicViewDefinitions(
        this IServiceCollection services,
        IDictionary<string, string> definitions)
    {
        services.AddSingleton<ISerializedViewDefinitionStore>(
            _ => new InMemorySerializedViewDefinitionSource(definitions));
        services.AddSingleton<IViewDefinitionStore, CachedViewDefinitionStore>();

        return services;
    }

    public static IServiceCollection AddDynamicViewDefinitionFolder(
        this IServiceCollection services,
        string folderPath)
    {
        services.AddSingleton<ISerializedViewDefinitionStore>(_ => new JsonFileSerializedViewDefinitionSource(folderPath));
        services.AddSingleton<IViewDefinitionStore, CachedViewDefinitionStore>();

        return services;
    }

    public static IServiceCollection AddDynamicViewActionHandler<T>(this IServiceCollection services)
        where T : class, IViewActionHandler
    {
        services.AddTransient<IViewActionHandler, T>();
        return services;
    }
}
