using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class DeliveryReceipt
{
    public Guid Id { get; } = Guid.NewGuid();

    public Guid JobId { get; }

    public string ReferenceNumber { get; }
    public string InvoiceNumber { get; }

    public string PickupCompany { get; }
    public string PickupAddress { get; }
    public string DeliveryCompany { get; }
    public string DeliveryAddress { get; }
    public string LoadDescription { get; }

    public RateType RateType { get; }
    public decimal RateValue { get; }
    public int Quantity { get; }
    public decimal Total { get; }

    public string ReceiverName { get; }
    public DateTime DeliveredAtUtc { get; }

    public string SignatureJson { get; }

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
        int quantity,
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
