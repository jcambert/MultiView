using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using MultiView.DynamicViews.Core.Abstractions;

namespace MultiView.DynamicViews.Core.Views;

public sealed class DefaultViewActionDispatcher : IViewActionDispatcher
{
    private readonly IEnumerable<IViewActionHandler> _handlers;

    public DefaultViewActionDispatcher(IEnumerable<IViewActionHandler> handlers)
    {
        _handlers = handlers;
    }

    public Task DispatchAsync(ViewActionContext context, CancellationToken cancellationToken = default)
    {
        IViewActionHandler? selected = _handlers.FirstOrDefault(handler =>
            string.Equals(handler.ActionName, context.Action.Name, StringComparison.OrdinalIgnoreCase)
            && handler.CanHandle(context));

        if (selected is null)
        {
            throw new InvalidOperationException($"No action handler registered for '{context.Action.Name}'.");
        }

        return selected.HandleAsync(context, cancellationToken);
    }
}
