using Haulory.Domain.Enums;

namespace Haulory.Contracts.Jobs;

public class CreateJobRequestDto
{
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

    public string ReferenceNumber { get; set; } = string.Empty;
    public string LoadDescription { get; set; } = string.Empty;

    public RateType RateType { get; set; }
    public decimal RateValue { get; set; }
    public decimal Quantity { get; set; }

    public Guid? DriverId { get; set; }
    public Guid? VehicleAssetId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    public List<Guid> TrailerAssetIds { get; set; } = new();
}