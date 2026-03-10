namespace Haulory.Api.Contracts.Reports;

public sealed class DeliveryReceiptDto
{
    public Guid Id { get; set; }
    public Guid JobId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;

    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;

    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public string LoadDescription { get; set; } = string.Empty;

    public string? ReceiverName { get; set; }
    public DateTime DeliveredAtUtc { get; set; }

    public string RateType { get; set; } = string.Empty;
    public decimal RateValue { get; set; }
    public decimal Quantity { get; set; }
    public decimal Total { get; set; }
}