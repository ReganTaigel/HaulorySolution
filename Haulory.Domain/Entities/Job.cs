using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class Job
{
    public Guid Id { get; } = Guid.NewGuid();

    // Pickup
    public string PickupCompany { get; }
    public string PickupAddress { get; }

    // Delivery
    public string DeliveryCompany { get; }
    public string DeliveryAddress { get; }

    // Load
    public string ReferenceNumber { get; }
    public string LoadDescription { get; }

    // Billing
    public string InvoiceNumber { get; }
    public RateType RateType { get; }
    public decimal RateValue { get; }
    public int Quantity { get; }

    public decimal Total => RateValue * Quantity;

    public DateTime CreatedAt { get; } = DateTime.UtcNow;

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
        int quantity)
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
    }
}
