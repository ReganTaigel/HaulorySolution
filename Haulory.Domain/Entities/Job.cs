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

    // Assigned to sub-user account (optional)
    public Guid? AssignedToUserId { get; private set; }

    // Optional linkage to driver and vehicle assets
    public Guid? DriverId { get; private set; }
    public Guid? VehicleAssetId { get; private set; } // Power unit asset id

    // ✅ Trailer assignments (max 2) stored via join table
    public IReadOnlyCollection<JobTrailerAssignment> TrailerAssignments => _trailerAssignments;
    private readonly List<JobTrailerAssignment> _trailerAssignments = new();

    public JobStatus Status { get; private set; } = JobStatus.Active;

    // Sub-user completion inputs
    public int? WaitTimeMinutes { get; private set; }
    public string? DamageNotes { get; private set; }

    // Optional: who completed it (useful for audit)
    public Guid? DeliveredByUserId { get; private set; }

    // Derived
    public bool RequiresReview =>
        (WaitTimeMinutes.HasValue && WaitTimeMinutes.Value > 0)
        || !string.IsNullOrWhiteSpace(DamageNotes);

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

    #region Client (Bill To)

    public string ClientCompanyName { get; private set; } = string.Empty;
    public string? ClientContactName { get; private set; }
    public string? ClientEmail { get; private set; }
    public string ClientAddressLine1 { get; private set; } = string.Empty;
    public string ClientCity { get; private set; } = string.Empty;
    public string ClientCountry { get; private set; } = string.Empty;

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

    #region Signatures / Delivery Confirmation

    // Delivery signature (POD)
    public string? ReceiverName { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public string? DeliverySignatureJson { get; private set; }

    // Delivered only when we have both a timestamp and signature data
    public bool IsDelivered =>
        DeliveredAtUtc != null && !string.IsNullOrWhiteSpace(DeliverySignatureJson);

    // Pickup signature (optional - safe to include now even if UI doesn’t use it yet)
    public string? PickupSignatureJson { get; private set; }
    public string? PickupSignedByName { get; private set; }
    public DateTime? PickupSignedAtUtc { get; private set; }

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
        Guid jobId,
        Guid ownerUserId,

        // Client (Bill To)
        string clientCompanyName,
        string? clientContactName,
        string? clientEmail,
        string clientAddressLine1,
        string clientCity,
        string clientCountry,

        // Pickup / Delivery (operational)
        string pickupCompany,
        string pickupAddress,
        string deliveryCompany,
        string deliveryAddress,

        // Job
        string referenceNumber,
        string loadDescription,
        string invoiceNumber,

        // Pricing
        RateType rateType,
        decimal rateValue,
        decimal quantity,

        // Ordering / Allocation
        int sortOrder,
        Guid? driverId = null,
        Guid? vehicleAssetId = null)
    {
        if (jobId == Guid.Empty)
            throw new ArgumentException("JobId required.", nameof(jobId));

        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("OwnerUserId required.");

        Id = jobId;
        OwnerUserId = ownerUserId;

        // Client (Bill To)
        ClientCompanyName = clientCompanyName?.Trim() ?? string.Empty;
        ClientContactName = string.IsNullOrWhiteSpace(clientContactName) ? null : clientContactName.Trim();
        ClientEmail = string.IsNullOrWhiteSpace(clientEmail) ? null : clientEmail.Trim();
        ClientAddressLine1 = clientAddressLine1?.Trim() ?? string.Empty;
        ClientCity = clientCity?.Trim() ?? string.Empty;
        ClientCountry = clientCountry?.Trim() ?? string.Empty;

        // Pickup / Delivery
        PickupCompany = pickupCompany?.Trim() ?? string.Empty;
        PickupAddress = pickupAddress?.Trim() ?? string.Empty;
        DeliveryCompany = deliveryCompany?.Trim() ?? string.Empty;
        DeliveryAddress = deliveryAddress?.Trim() ?? string.Empty;

        // Load
        ReferenceNumber = referenceNumber?.Trim() ?? string.Empty;
        LoadDescription = loadDescription?.Trim() ?? string.Empty;

        // Invoice
        InvoiceNumber = invoiceNumber?.Trim() ?? string.Empty;

        // Pricing
        SetRateInternal(rateType, rateValue, quantity);

        SortOrder = sortOrder;
        DriverId = driverId;
        VehicleAssetId = vehicleAssetId;
    }

    #endregion

    #region Pricing Logic

    private void SetRateInternal(RateType rateType, decimal rateValue, decimal quantity)
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

        Quantity = (rateType == RateType.FixedFee || rateType == RateType.Percentage)
            ? 1m
            : quantity;
    }

    // Keep public method if older code calls it
    public void SetRate(RateType rateType, decimal rateValue, decimal quantity)
        => SetRateInternal(rateType, rateValue, quantity);

    #endregion

    #region Assignment

    public void AssignDriver(Guid? driverId) => DriverId = driverId;

    public void AssignVehicle(Guid? vehicleAssetId) => VehicleAssetId = vehicleAssetId;

    // Later workflow: job assigned to a sub-user account
    public void AssignToSubUser(Guid? subUserId) => AssignedToUserId = subUserId;

    // ✅ Max 2 trailers
    public void SetTrailers(IEnumerable<Guid>? trailerAssetIds)
    {
        _trailerAssignments.Clear();

        if (trailerAssetIds == null) return;

        var distinct = trailerAssetIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .Take(2) // max two trailers
            .ToList();

        for (int i = 0; i < distinct.Count; i++)
            _trailerAssignments.Add(new JobTrailerAssignment(Id, distinct[i], i + 1));
    }

    #endregion

    #region Ordering

    public void SetSortOrder(int sortOrder) => SortOrder = sortOrder;

    #endregion

    #region Delivery

    public void CompleteDelivery(
        Guid deliveredByUserId,
        string receiverName,
        string signatureJson,
        int? waitTimeMinutes,
        string? damageNotes)
    {
        ReceiverName = NameFormatter.ToTitleCase(receiverName);

        DeliverySignatureJson = string.IsNullOrWhiteSpace(signatureJson)
            ? null
            : signatureJson.Trim();

        DeliveredAtUtc = DateTime.UtcNow;
        DeliveredByUserId = deliveredByUserId;

        WaitTimeMinutes = waitTimeMinutes;
        DamageNotes = string.IsNullOrWhiteSpace(damageNotes) ? null : damageNotes.Trim();

        // Your rule:
        Status = RequiresReview
            ? JobStatus.DeliveredPendingReview
            : JobStatus.Completed;
    }

    #endregion
}

#endregion