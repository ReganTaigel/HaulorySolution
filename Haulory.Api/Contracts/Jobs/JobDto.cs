namespace Haulory.Api.Contracts.Jobs;

public sealed class JobDto
{
    public Guid Id { get; set; }

    public Guid OwnerUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? VehicleAssetId { get; set; }

    public string Status { get; set; } = string.Empty;

    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;

    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    public string ReferenceNumber { get; set; } = string.Empty;
    public string LoadDescription { get; set; } = string.Empty;

    public string ClientCompanyName { get; set; } = string.Empty;
    public string? ClientContactName { get; set; }
    public string? ClientEmail { get; set; }
    public string ClientAddressLine1 { get; set; } = string.Empty;
    public string ClientCity { get; set; } = string.Empty;
    public string ClientCountry { get; set; } = string.Empty;

    public string InvoiceNumber { get; set; } = string.Empty;

    public string RateType { get; set; } = string.Empty;
    public decimal RateValue { get; set; }
    public decimal Quantity { get; set; }
    public string QuantityUnit { get; set; } = string.Empty;
    public decimal Total { get; set; }

    public string? ReceiverName { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public bool IsDelivered { get; set; }

    public int SortOrder { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<Guid> TrailerAssetIds { get; set; } = new();
}