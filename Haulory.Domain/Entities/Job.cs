using System.Text.Json.Serialization;
using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class Job
{
    public Guid Id { get; init; }  

    // Pickup
    public string PickupCompany { get; init; }
    public string PickupAddress { get; init; }

    // Delivery
    public string DeliveryCompany { get; init; }
    public string DeliveryAddress { get; init; }

    // Signature
    public string? ReceiverName { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public string? DeliverySignatureJson { get; private set; }

    public bool IsDelivered => DeliveredAtUtc != null && !string.IsNullOrWhiteSpace(DeliverySignatureJson);

    // Load
    public string ReferenceNumber { get; init; }
    public string LoadDescription { get; init; }

    // Billing
    public string InvoiceNumber { get; init; }
    public RateType RateType { get; init; }
    public decimal RateValue { get; init; }
    public int Quantity { get; init; }

    public decimal Total => RateValue * Quantity;
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    // Manual ordering
    public int SortOrder { get; private set; }

    // Interface summary card
    public string CardSummary =>
    $"{PickupCompany} → {DeliveryCompany}\n" +
    $"{DeliveryAddress}\n" +
    $"{LoadDescription}";

    // Used by your app when creating new jobs
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
        Id = Guid.NewGuid(); // app sets it here

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

    // Used when manually reordering jobs
    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    // Used for delivery completion
    public void MarkDelivered(string receiverName, string signatureJson)
    {
        ReceiverName = receiverName?.Trim();
        DeliverySignatureJson = signatureJson;
        DeliveredAtUtc = DateTime.UtcNow;
    }
}
