using MultiView.DynamicViews.Domain.Model;

namespace MultiView.DynamicViews.Core.Abstractions;

public sealed record ViewActionContext(
    DynamicViewDefinition ViewDefinition,
    ViewActionDefinition Action,
    IReadOnlyList<object> SelectedRecords,
    object? CurrentRecord,
    IServiceProvider Services,
    IDictionary<string, object?> Metadata);

public interface IViewActionHandler
{
    string ActionName { get; }

    bool CanHandle(ViewActionContext context);

    Task HandleAsync(ViewActionContext context, CancellationToken cancellationToken = default);
}

public interface IViewActionDispatcher
{
    Task DispatchAsync(ViewActionContext context, CancellationToken cancellationToken = default);
}
