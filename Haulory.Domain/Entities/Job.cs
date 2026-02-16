using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class Job
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Ownership / assignment
    public Guid OwnerUserId { get; private set; }            // MAIN owner
    public Guid? AssignedToUserId { get; private set; }      // SUB (later)

    public Guid? DriverId { get; private set; }
    public Guid? VehicleAssetId { get; private set; }        // Power unit asset id

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
    public decimal Quantity { get; private set; }            // <-- decimal
    public QuantityUnit QuantityUnit { get; private set; } = QuantityUnit.None;

    public decimal Total => RateType switch
    {
        RateType.FixedFee => RateValue,
        _ => RateValue * Quantity
    };

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

    // Ordering
    public int SortOrder { get; private set; }

    public Job() { } // EF

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
        if (ownerUserId == Guid.Empty) throw new ArgumentException("OwnerUserId required.");

        OwnerUserId = ownerUserId;

        PickupCompany = pickupCompany;
        PickupAddress = pickupAddress;
        DeliveryCompany = deliveryCompany;
        DeliveryAddress = deliveryAddress;

        ReferenceNumber = referenceNumber;
        LoadDescription = loadDescription;

        InvoiceNumber = invoiceNumber;

        SetRate(rateType, rateValue, quantity);

        SortOrder = sortOrder;

        DriverId = driverId;
        VehicleAssetId = vehicleAssetId;
    }

    public void SetRate(RateType rateType, decimal rateValue, decimal quantity)
    {
        RateType = rateType;
        RateValue = rateValue;

        QuantityUnit = rateType switch
        {
            RateType.PerLoad => QuantityUnit.Load,
            RateType.PerPallet => QuantityUnit.Pallet,
            RateType.PerTonne => QuantityUnit.Tonne,
            RateType.PerKm => QuantityUnit.Km,
            RateType.Hourly => QuantityUnit.Hour,
            _ => QuantityUnit.None
        };

        Quantity = (rateType == RateType.FixedFee || rateType == RateType.Percentage) ? 1m : quantity;
    }

    public void AssignDriver(Guid? driverId) => DriverId = driverId;
    public void AssignVehicle(Guid? vehicleAssetId) => VehicleAssetId = vehicleAssetId;

    // later:
    public void AssignToSubUser(Guid? subUserId) => AssignedToUserId = subUserId;

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    public void MarkDelivered(string receiverName, string signatureJson)
    {
        ReceiverName = receiverName?.Trim();
        DeliverySignatureJson = signatureJson;
        DeliveredAtUtc = DateTime.UtcNow;
    }
}
