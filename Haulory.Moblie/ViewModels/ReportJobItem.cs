using Haulory.Domain.Enums;

namespace Haulory.Moblie.ViewModels;

public class ReportJobItem
{
    public Guid JobId { get; init; }
    public string ReferenceNumber { get; init; } = "";
    public string InvoiceNumber { get; init; } = "";
    public string DeliveryCompany { get; init; } = "";
    public string DeliveryAddress { get; init; } = "";
    public string ReceiverName { get; init; } = "";
    public DateTime DeliveredAtUtc { get; init; }

    public RateType RateType { get; init; }
    public decimal RateValue { get; init; }
    public int Quantity { get; init; }
    public decimal Total { get; init; }
}
