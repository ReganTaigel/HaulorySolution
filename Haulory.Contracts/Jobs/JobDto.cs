namespace Haulory.Contracts.Jobs;

public sealed class JobDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }

    // IMPORTANT for grouping
    public Guid? AssignedToUserId { get; set; }

    // IMPORTANT for ordering
    public int SortOrder { get; set; }

    public Guid? CustomerId { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;
    public string LoadDescription { get; set; } = string.Empty;

    public string ClientCompanyName { get; set; } = string.Empty;
    public string? ClientContactName { get; set; }
    public string? ClientEmail { get; set; }
    public string ClientAddressLine1 { get; set; } = string.Empty;
    public string ClientCity { get; set; } = string.Empty;
    public string ClientCountry { get; set; } = string.Empty;

    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;

    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public string InvoiceNumber { get; set; } = string.Empty;

    public string RateType { get; set; } = string.Empty;
    public decimal RateValue { get; set; }
    public decimal Quantity { get; set; }
    public decimal Total { get; set; }

    public string Status { get; set; } = string.Empty;

    public Guid? DriverId { get; set; }
    public Guid? VehicleAssetId { get; set; }

    // trailer IDs returned by API
    public List<Guid> TrailerAssetIds { get; set; } = new();

    public string? ReceiverName { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public string? DeliverySignatureJson { get; set; }

    public int? WaitTimeMinutes { get; set; }
    public string? DamageNotes { get; set; }
    public bool RequiresReview { get; set; }

    public bool IsDelivered =>
        DeliveredAtUtc.HasValue && !string.IsNullOrWhiteSpace(DeliverySignatureJson);
}