using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class DeliveryReceipt
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid JobId { get; private set; }

    public string ReferenceNumber { get; private set; } = string.Empty;
    public string InvoiceNumber { get; private set; } = string.Empty;

    public string PickupCompany { get; private set; } = string.Empty;
    public string PickupAddress { get; private set; } = string.Empty;
    public string DeliveryCompany { get; private set; } = string.Empty;
    public string DeliveryAddress { get; private set; } = string.Empty;
    public string LoadDescription { get; private set; } = string.Empty;

    public RateType RateType { get; private set; }
    public decimal RateValue { get; private set; }
    public decimal Quantity { get; private set; }
    public decimal Total { get; private set; }

    public string ReceiverName { get; private set; } = string.Empty;
    public DateTime DeliveredAtUtc { get; private set; }

    public string SignatureJson { get; private set; } = string.Empty;

    // Required by EF
    public DeliveryReceipt() { }

    public DeliveryReceipt(
        Guid jobId,
        string referenceNumber,
        string invoiceNumber,
        string pickupCompany,
        string pickupAddress,
        string deliveryCompany,
        string deliveryAddress,
        string loadDescription,
        RateType rateType,
        decimal rateValue,
        decimal quantity,
        decimal total,
        string receiverName,
        DateTime deliveredAtUtc,
        string signatureJson)
    {
        JobId = jobId;
        ReferenceNumber = referenceNumber;
        InvoiceNumber = invoiceNumber;

        PickupCompany = pickupCompany;
        PickupAddress = pickupAddress;
        DeliveryCompany = deliveryCompany;
        DeliveryAddress = deliveryAddress;
        LoadDescription = loadDescription;

        RateType = rateType;
        RateValue = rateValue;
        Quantity = quantity;
        Total = total;

        ReceiverName = receiverName;
        DeliveredAtUtc = deliveredAtUtc;
        SignatureJson = signatureJson;
    }
}
