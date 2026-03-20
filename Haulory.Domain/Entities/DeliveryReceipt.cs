using Haulory.Domain.Enums;
using Haulory.Domain.Helpers;

namespace Haulory.Domain.Entities;

public class DeliveryReceipt
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid OwnerUserId { get; private set; }
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

    public decimal Subtotal { get; private set; }

    public bool GstEnabled { get; private set; }
    public decimal GstRatePercent { get; private set; }
    public decimal GstAmount { get; private set; }

    public bool FuelSurchargeEnabled { get; private set; }
    public decimal FuelSurchargePercent { get; private set; }
    public decimal FuelSurchargeAmount { get; private set; }

    public decimal Total { get; private set; }

    public string ReceiverName { get; private set; } = string.Empty;
    public DateTime DeliveredAtUtc { get; private set; }
    public string SignatureJson { get; private set; } = string.Empty;

    public string ClientCompanyName { get; private set; } = string.Empty;
    public string? ClientContactName { get; private set; }
    public string? ClientEmail { get; private set; }
    public string ClientAddressLine1 { get; private set; } = string.Empty;
    public string ClientCity { get; private set; } = string.Empty;
    public string ClientCountry { get; private set; } = string.Empty;

    public int? WaitTimeMinutes { get; private set; }
    public string? DamageNotes { get; private set; }

    public bool ShowDamageNotesOnPod { get; set; }
    public bool ShowWaitTimeOnPod { get; set; }
    private DeliveryReceipt() { }

    public DeliveryReceipt(
        Guid ownerUserId,
        Guid jobId,
        string clientCompanyName,
        string? clientContactName,
        string? clientEmail,
        string clientAddressLine1,
        string clientCity,
        string clientCountry,
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
        decimal subtotal,
        bool gstEnabled,
        decimal gstRatePercent,
        decimal gstAmount,
        bool fuelSurchargeEnabled,
        decimal fuelSurchargePercent,
        decimal fuelSurchargeAmount,
        decimal total,
        string receiverName,
        DateTime deliveredAtUtc,
        string signatureJson,
        int? waitTimeMinutes = null,
        string? damageNotes = null)
    {
        if (ownerUserId == Guid.Empty) throw new ArgumentException("OwnerUserId required.");
        if (jobId == Guid.Empty) throw new ArgumentException("JobId required.");

        OwnerUserId = ownerUserId;
        JobId = jobId;

        ClientCompanyName = clientCompanyName?.Trim() ?? string.Empty;
        ClientContactName = string.IsNullOrWhiteSpace(clientContactName) ? null : clientContactName.Trim();
        ClientEmail = string.IsNullOrWhiteSpace(clientEmail) ? null : clientEmail.Trim().ToLowerInvariant();
        ClientAddressLine1 = clientAddressLine1?.Trim() ?? string.Empty;
        ClientCity = clientCity?.Trim() ?? string.Empty;
        ClientCountry = clientCountry?.Trim() ?? "New Zealand";

        ReferenceNumber = referenceNumber?.Trim() ?? string.Empty;
        InvoiceNumber = invoiceNumber?.Trim() ?? string.Empty;

        PickupCompany = pickupCompany?.Trim() ?? string.Empty;
        PickupAddress = pickupAddress?.Trim() ?? string.Empty;

        DeliveryCompany = deliveryCompany?.Trim() ?? string.Empty;
        DeliveryAddress = deliveryAddress?.Trim() ?? string.Empty;

        LoadDescription = loadDescription?.Trim() ?? string.Empty;

        RateType = rateType;
        RateValue = rateValue;
        Quantity = quantity;

        Subtotal = subtotal;

        GstEnabled = gstEnabled;
        GstRatePercent = gstRatePercent;
        GstAmount = gstAmount;

        FuelSurchargeEnabled = fuelSurchargeEnabled;
        FuelSurchargePercent = fuelSurchargePercent;
        FuelSurchargeAmount = fuelSurchargeAmount;

        Total = total;

        ReceiverName = NameFormatter.ToTitleCase(receiverName) ?? string.Empty;
        DeliveredAtUtc = deliveredAtUtc.Kind == DateTimeKind.Utc
            ? deliveredAtUtc
            : DateTime.SpecifyKind(deliveredAtUtc, DateTimeKind.Utc);

        SignatureJson = signatureJson?.Trim() ?? string.Empty;

        WaitTimeMinutes = waitTimeMinutes;
        DamageNotes = string.IsNullOrWhiteSpace(damageNotes) ? null : damageNotes.Trim();
    }
    public void RefreshSnapshot(
    string clientCompanyName,
    string? clientContactName,
    string? clientEmail,
    string clientAddressLine1,
    string clientCity,
    string clientCountry,
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
    decimal subtotal,
    bool gstEnabled,
    decimal gstRatePercent,
    decimal gstAmount,
    bool fuelSurchargeEnabled,
    decimal fuelSurchargePercent,
    decimal fuelSurchargeAmount,
    decimal total,
    string receiverName,
    DateTime deliveredAtUtc,
    string signatureJson,
    int? waitTimeMinutes = null,
    string? damageNotes = null)
    {
        ClientCompanyName = clientCompanyName?.Trim() ?? string.Empty;
        ClientContactName = string.IsNullOrWhiteSpace(clientContactName) ? null : clientContactName.Trim();
        ClientEmail = string.IsNullOrWhiteSpace(clientEmail) ? null : clientEmail.Trim().ToLowerInvariant();
        ClientAddressLine1 = clientAddressLine1?.Trim() ?? string.Empty;
        ClientCity = clientCity?.Trim() ?? string.Empty;
        ClientCountry = clientCountry?.Trim() ?? "New Zealand";

        ReferenceNumber = referenceNumber?.Trim() ?? string.Empty;
        InvoiceNumber = invoiceNumber?.Trim() ?? string.Empty;

        PickupCompany = pickupCompany?.Trim() ?? string.Empty;
        PickupAddress = pickupAddress?.Trim() ?? string.Empty;

        DeliveryCompany = deliveryCompany?.Trim() ?? string.Empty;
        DeliveryAddress = deliveryAddress?.Trim() ?? string.Empty;

        LoadDescription = loadDescription?.Trim() ?? string.Empty;

        RateType = rateType;
        RateValue = rateValue;
        Quantity = quantity;

        Subtotal = subtotal;

        GstEnabled = gstEnabled;
        GstRatePercent = gstRatePercent;
        GstAmount = gstAmount;

        FuelSurchargeEnabled = fuelSurchargeEnabled;
        FuelSurchargePercent = fuelSurchargePercent;
        FuelSurchargeAmount = fuelSurchargeAmount;

        Total = total;

        ReceiverName = NameFormatter.ToTitleCase(receiverName) ?? string.Empty;
        DeliveredAtUtc = deliveredAtUtc.Kind == DateTimeKind.Utc
            ? deliveredAtUtc
            : DateTime.SpecifyKind(deliveredAtUtc, DateTimeKind.Utc);

        SignatureJson = signatureJson?.Trim() ?? string.Empty;

        WaitTimeMinutes = waitTimeMinutes;
        DamageNotes = string.IsNullOrWhiteSpace(damageNotes) ? null : damageNotes.Trim();
    }
}