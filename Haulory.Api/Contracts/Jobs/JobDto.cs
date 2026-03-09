namespace Haulory.Mobile.Contracts.Jobs;

public sealed class JobDto
{
    public Guid Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public string? ClientCompanyName { get; set; }
    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? InvoiceNumber { get; set; }
    public string? ReceiverName { get; set; }
    public DateTime? DeliveredAtUtc { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? VehicleAssetId { get; set; }
    public List<Guid> TrailerAssetIds { get; set; } = new();
}