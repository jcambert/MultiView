namespace MultiView.DynamicViews.Sample.Models;

public sealed class SaleOrder
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Number { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string SalesTeam { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "EUR";

    public string Status { get; set; } = "Draft";

    public bool Confirmed { get; set; }

    public string Origin { get; set; } = "Web";

    public string Notes { get; set; } = string.Empty;
}