using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Sample.Models;
using Microsoft.AspNetCore.Components;

namespace MultiView.DynamicViews.Sample.Actions;

public sealed class ToggleConfirmActionHandler : IViewActionHandler
{
    private readonly IDataProvider<SaleOrder> _dataProvider;

    public ToggleConfirmActionHandler(IDataProvider<SaleOrder> dataProvider)
    {
        _dataProvider = dataProvider;
    }

    public string ActionName => "toggle_confirm";

    public bool CanHandle(ViewActionContext context) => context.CurrentRecord is SaleOrder;

    public async Task HandleAsync(ViewActionContext context, CancellationToken cancellationToken = default)
    {
        if (context.CurrentRecord is not SaleOrder order)
        {
            return;
        }

        order.Confirmed = !order.Confirmed;
        order.Status = order.Confirmed ? "Confirmed" : "Draft";
        await _dataProvider.SaveAsync(order, cancellationToken);
    }
}