using MultiView.DynamicViews.Core.Abstractions;
using MultiView.DynamicViews.Sample.Models;

namespace MultiView.DynamicViews.Sample.Data;

public sealed class SaleOrderDataProvider : IDataProvider<SaleOrder>
{
    private readonly List<SaleOrder> _records =
    [
        new()
        {
            Number = "SO-2026-001",
            CustomerName = "Alpha Industries",
            SalesTeam = "Enterprise",
            OrderDate = new DateTime(2026, 3, 10),
            Amount = 12000m,
            Currency = "EUR",
            Status = "Draft",
            Confirmed = false,
            Origin = "Web",
            Notes = "Commande initiale"
        },
        new()
        {
            Number = "SO-2026-002",
            CustomerName = "Delta Services",
            SalesTeam = "PME",
            OrderDate = new DateTime(2026, 3, 8),
            Amount = 9800m,
            Currency = "EUR",
            Status = "Confirmed",
            Confirmed = true,
            Origin = "Partner",
            Notes = "Paiement reçu"
        },
        new()
        {
            Number = "SO-2026-003",
            CustomerName = "Global Tech",
            SalesTeam = "Strategic",
            OrderDate = new DateTime(2026, 3, 5),
            Amount = 5200m,
            Currency = "EUR",
            Status = "Cancelled",
            Confirmed = false,
            Origin = "Referral",
            Notes = "Prospect fermé"
        }
    ];

    public ValueTask<IReadOnlyList<SaleOrder>> ListAsync(CancellationToken cancellationToken = default)
    {
        return ValueTask.FromResult((IReadOnlyList<SaleOrder>)_records);
    }

    public ValueTask<SaleOrder?> GetByIdAsync(object id, CancellationToken cancellationToken = default)
    {
        if (id is Guid guid)
        {
            return ValueTask.FromResult(_records.FirstOrDefault(x => x.Id == guid));
        }

        return ValueTask.FromResult<SaleOrder?>(null);
    }

    public ValueTask SaveAsync(SaleOrder model, CancellationToken cancellationToken = default)
    {
        int index = _records.FindIndex(item => item.Id == model.Id);
        if (index >= 0)
        {
            _records[index] = model;
            return ValueTask.CompletedTask;
        }

        model.Id = Guid.NewGuid();
        _records.Add(model);
        return ValueTask.CompletedTask;
    }

    public ValueTask DeleteAsync(SaleOrder model, CancellationToken cancellationToken = default)
    {
        _records.RemoveAll(item => item.Id == model.Id);
        return ValueTask.CompletedTask;
    }
}