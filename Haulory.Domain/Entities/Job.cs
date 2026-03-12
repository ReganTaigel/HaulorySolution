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

    public Guid OwnerUserId { get; private set; }
    public Guid? AssignedToUserId { get; private set; }

    public Guid? DriverId { get; private set; }
    public Guid? VehicleAssetId { get; private set; } // Power unit asset id

    private readonly List<JobTrailerAssignment> _trailerAssignments = new();
    public IReadOnlyCollection<JobTrailerAssignment> TrailerAssignments => _trailerAssignments.AsReadOnly();

    public JobStatus Status { get; private set; } = JobStatus.Active;

    public int? WaitTimeMinutes { get; private set; }
    public string? DamageNotes { get; private set; }
    public Guid? DeliveredByUserId { get; private set; }

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
    public decimal Quantity { get; private set; }
    public QuantityUnit QuantityUnit { get; private set; } = QuantityUnit.None;

    public decimal Total => RateType switch
    {
        RateType.FixedFee => RateValue,
        _ => RateValue * Quantity
    };

    #endregion

    #region Signatures / Delivery Confirmation

    public string? ReceiverName { get; private set; }
    public DateTime? DeliveredAtUtc { get; private set; }
    public string? DeliverySignatureJson { get; private set; }

    public bool IsDelivered =>
        DeliveredAtUtc != null && !string.IsNullOrWhiteSpace(DeliverySignatureJson);

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

    private Job() { }

    public Job(
        Guid jobId,
        Guid ownerUserId,

        string clientCompanyName,
        string? clientContactName,
        string? clientEmail,
        string clientAddressLine1,
        string clientCity,
        string clientCountry,

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
        if (jobId == Guid.Empty)
            throw new ArgumentException("JobId required.", nameof(jobId));

        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("OwnerUserId required.");

        Id = jobId;
        OwnerUserId = ownerUserId;

        ClientCompanyName = clientCompanyName?.Trim() ?? string.Empty;
        ClientContactName = string.IsNullOrWhiteSpace(clientContactName) ? null : clientContactName.Trim();
        ClientEmail = string.IsNullOrWhiteSpace(clientEmail) ? null : clientEmail.Trim();
        ClientAddressLine1 = clientAddressLine1?.Trim() ?? string.Empty;
        ClientCity = clientCity?.Trim() ?? string.Empty;
        ClientCountry = clientCountry?.Trim() ?? string.Empty;

        PickupCompany = pickupCompany?.Trim() ?? string.Empty;
        PickupAddress = pickupAddress?.Trim() ?? string.Empty;
        DeliveryCompany = deliveryCompany?.Trim() ?? string.Empty;
        DeliveryAddress = deliveryAddress?.Trim() ?? string.Empty;

        ReferenceNumber = referenceNumber?.Trim() ?? string.Empty;
        LoadDescription = loadDescription?.Trim() ?? string.Empty;

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

    public void SetRate(RateType rateType, decimal rateValue, decimal quantity)
        => SetRateInternal(rateType, rateValue, quantity);

    #endregion

    #region Assignment

    public void AssignDriver(Guid? driverId) => DriverId = driverId;

    public void AssignVehicle(Guid? vehicleAssetId) => VehicleAssetId = vehicleAssetId;

    public void AssignToSubUser(Guid? subUserId) => AssignedToUserId = subUserId;

    public void SetTrailers(IEnumerable<Guid>? trailerAssetIds)
    {
        _trailerAssignments.Clear();

        if (trailerAssetIds == null)
            return;

        var distinct = trailerAssetIds
            .Where(id => id != Guid.Empty)
            .Distinct()
            .ToList();

        if (distinct.Count > 2)
            throw new InvalidOperationException("A maximum of 2 trailers can be assigned to a job.");

        for (int i = 0; i < distinct.Count; i++)
        {
            _trailerAssignments.Add(new JobTrailerAssignment(
                Id,
                distinct[i],
                i + 1));
        }
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

        Status = RequiresReview
            ? JobStatus.DeliveredPendingReview
            : JobStatus.Completed;
    }

    #endregion
    public void UpdateDetails(
    string clientCompanyName,
    string? clientContactName,
    string? clientEmail,
    string clientAddressLine1,
    string clientCity,
    string clientCountry,
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
    Guid? driverId,
    Guid? vehicleAssetId)
    {
        ClientCompanyName = clientCompanyName.Trim();
        ClientContactName = string.IsNullOrWhiteSpace(clientContactName) ? null : clientContactName.Trim();
        ClientEmail = string.IsNullOrWhiteSpace(clientEmail) ? null : clientEmail.Trim();
        ClientAddressLine1 = clientAddressLine1.Trim();
        ClientCity = clientCity.Trim();
        ClientCountry = clientCountry.Trim();

        PickupCompany = pickupCompany.Trim();
        PickupAddress = pickupAddress.Trim();
        DeliveryCompany = deliveryCompany.Trim();
        DeliveryAddress = deliveryAddress.Trim();

        ReferenceNumber = referenceNumber?.Trim() ?? string.Empty;
        LoadDescription = loadDescription?.Trim() ?? string.Empty;

        RateType = rateType;
        RateValue = rateValue;
        Quantity = quantity;

        DriverId = driverId;
        VehicleAssetId = vehicleAssetId;
    }
}

#endregion