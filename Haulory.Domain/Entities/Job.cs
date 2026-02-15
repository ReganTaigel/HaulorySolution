using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class Job
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Pickup
    public string PickupCompany { get; private set; } = string.Empty;
    public string PickupAddress { get; private set; } = string.Empty;

    // Delivery
    public string DeliveryCompany { get; private set; } = string.Empty;
    public string DeliveryAddress { get; private set; } = string.Empty;

    // Signature
    public string? ReceiverName { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public string? DeliverySignatureJson { get; private set; }

    public bool IsDelivered => DeliveredAtUtc != null && !string.IsNullOrWhiteSpace(DeliverySignatureJson);

    // Load
    public string ReferenceNumber { get; private set; } = string.Empty;
    public string LoadDescription { get; private set; } = string.Empty;

    // Billing
    public string InvoiceNumber { get; private set; } = string.Empty;
    public RateType RateType { get; private set; }
    public decimal RateValue { get; private set; }
    public int Quantity { get; private set; }

    public decimal Total => RateValue * Quantity;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Manual ordering
    public int SortOrder { get; private set; }

    public string CardSummary =>
        $"{PickupCompany} → {DeliveryCompany}\n" +
        $"{DeliveryAddress}\n" +
        $"{LoadDescription}";

    // ✅ EF needs this
    public Job() { }

    public Job(
        string pickupCompany,
        string pickupAddress,
        string deliveryCompany,
        string deliveryAddress,
        string referenceNumber,
        string loadDescription,
        string invoiceNumber,
        RateType rateType,
        decimal rateValue,
        int quantity,
        int sortOrder)
    {
        PickupCompany = pickupCompany;
        PickupAddress = pickupAddress;
        DeliveryCompany = deliveryCompany;
        DeliveryAddress = deliveryAddress;
        ReferenceNumber = referenceNumber;
        LoadDescription = loadDescription;
        InvoiceNumber = invoiceNumber;
        RateType = rateType;
        RateValue = rateValue;
        Quantity = quantity;
        SortOrder = sortOrder;
    }

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    public void MarkDelivered(string receiverName, string signatureJson)
    {
        ReceiverName = receiverName?.Trim();
        DeliverySignatureJson = signatureJson;
        DeliveredAtUtc = DateTime.UtcNow;
    }
}
