using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

#region Entity: Delivery Receipt

public class DeliveryReceipt
{
    #region Identity

    // Unique identifier for the receipt
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Associated Job Id
    public Guid JobId { get; private set; }

    #endregion

    #region Job Snapshot Data

    // Snapshot fields copied from Job at time of delivery
    // Ensures historical accuracy even if Job changes later

    public string ReferenceNumber { get; private set; } = string.Empty;
    public string InvoiceNumber { get; private set; } = string.Empty;

    public string PickupCompany { get; private set; } = string.Empty;
    public string PickupAddress { get; private set; } = string.Empty;

    public string DeliveryCompany { get; private set; } = string.Empty;
    public string DeliveryAddress { get; private set; } = string.Empty;

    public string LoadDescription { get; private set; } = string.Empty;

    #endregion

    #region Pricing

    public RateType RateType { get; private set; }
    public decimal RateValue { get; private set; }
    public decimal Quantity { get; private set; }

    // Final calculated total stored at time of delivery
    public decimal Total { get; private set; }

    #endregion

    #region Delivery Confirmation

    // Name of person who received the load
    public string ReceiverName { get; private set; } = string.Empty;

    // Delivery timestamp (UTC)
    public DateTime DeliveredAtUtc { get; private set; }

    // Serialized signature data (e.g., JSON from mobile app)
    public string SignatureJson { get; private set; } = string.Empty;

    #endregion

    #region Constructors

    // Required by EF Core
    private DeliveryReceipt() { }

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

    #endregion
}

#endregion
