using Haulory.Domain.Enums;
using Haulory.Domain.Helpers;

namespace Haulory.Domain.Entities;

#region Entity: Job

public class Job
{
    #region Identity

    public Guid Id { get; private set; } = Guid.NewGuid();

    #endregion

    #region Ownership / Assignment

    // MAIN owner (tenant boundary)
    public Guid OwnerUserId { get; private set; }

    // Assigned to sub-user account (optional, later workflow)
    public Guid? AssignedToUserId { get; private set; }

    // Optional linkage to driver and vehicle assets
    public Guid? DriverId { get; private set; }
    public Guid? VehicleAssetId { get; private set; } // Power unit asset id

    #endregion

    #region Pickup

    public string PickupCompany { get; private set; } = string.Empty;
    public string PickupAddress { get; private set; } = string.Empty;

    #endregion

    #region Delivery

    public string DeliveryCompany { get; private set; } = string.Empty;
    public string DeliveryAddress { get; private set; } = string.Empty;

    #endregion

    #region Load

    public string ReferenceNumber { get; private set; } = string.Empty;
    public string LoadDescription { get; private set; } = string.Empty;

    #endregion

    #region Billing

    public string InvoiceNumber { get; private set; } = string.Empty;

    public RateType RateType { get; private set; }
    public decimal RateValue { get; private set; }

    // Quantity is decimal to support fractional hours/tonnes/etc.
    public decimal Quantity { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; } = QuantityUnit.None;

    // Calculated total (derived at runtime from rate + quantity)
    public decimal Total => RateType switch
    {
        RateType.FixedFee => RateValue,
        _ => RateValue * Quantity
    };

    #endregion

    #region Delivery Confirmation / Signature

    public string? ReceiverName { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public string? DeliverySignatureJson { get; private set; }

    // Delivered only when we have both a timestamp and signature data
    public bool IsDelivered =>
        DeliveredAtUtc != null && !string.IsNullOrWhiteSpace(DeliverySignatureJson);

    #endregion

    #region Auditing

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    #endregion

    #region Ordering

    public int SortOrder { get; private set; }

    #endregion

    #region Constructors

    // Required by EF Core
    private Job() { }

    public Job(
        Guid ownerUserId,
        string pickupCompany,
        string pickupAddress,
        string deliveryCompany,
        string deliveryAddress,
        string referenceNumber,
        string loadDescription,
        string invoiceNumber,
        RateType rateType,
        decimal rateValue,
        decimal quantity,
        int sortOrder,
        Guid? driverId = null,
        Guid? vehicleAssetId = null)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("OwnerUserId required.");

        OwnerUserId = ownerUserId;

        // Trim inputs for consistent storage/searching (do not title-case companies/addresses)
        PickupCompany = pickupCompany?.Trim() ?? string.Empty;
        PickupAddress = pickupAddress?.Trim() ?? string.Empty;

        DeliveryCompany = deliveryCompany?.Trim() ?? string.Empty;
        DeliveryAddress = deliveryAddress?.Trim() ?? string.Empty;

        ReferenceNumber = referenceNumber?.Trim() ?? string.Empty;
        LoadDescription = loadDescription?.Trim() ?? string.Empty;

        InvoiceNumber = invoiceNumber?.Trim() ?? string.Empty;

        SetRate(rateType, rateValue, quantity);

        SortOrder = sortOrder;

        DriverId = driverId;
        VehicleAssetId = vehicleAssetId;
    }

    #endregion

    #region Pricing Logic

    public void SetRate(RateType rateType, decimal rateValue, decimal quantity)
    {
        RateType = rateType;
        RateValue = rateValue;

        // Map billing type to display unit used by UI and reporting
        QuantityUnit = rateType switch
        {
            RateType.PerLoad => QuantityUnit.Load,
            RateType.PerPallet => QuantityUnit.Pallet,
            RateType.PerTonne => QuantityUnit.Tonne,
            RateType.PerKm => QuantityUnit.Km,
            RateType.Hourly => QuantityUnit.Hour,
            _ => QuantityUnit.None
        };

        // FixedFee & Percentage do not depend on "quantity"
        Quantity = (rateType == RateType.FixedFee || rateType == RateType.Percentage)
            ? 1m
            : quantity;
    }

    #endregion

    #region Assignment

    public void AssignDriver(Guid? driverId) => DriverId = driverId;

    public void AssignVehicle(Guid? vehicleAssetId) => VehicleAssetId = vehicleAssetId;

    // Later workflow: job assigned to a sub-user account
    public void AssignToSubUser(Guid? subUserId) => AssignedToUserId = subUserId;

    #endregion

    #region Ordering

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    #endregion

    #region Delivery

    public void MarkDelivered(string receiverName, string signatureJson)
    {
        // Normalize receiver name for consistent display/auditing across the app
        ReceiverName = NameFormatter.ToTitleCase(receiverName);

        // Keep signature payload intact but avoid accidental leading/trailing whitespace
        DeliverySignatureJson = string.IsNullOrWhiteSpace(signatureJson)
            ? null
            : signatureJson.Trim();

        DeliveredAtUtc = DateTime.UtcNow;
    }

    #endregion
}

#endregion