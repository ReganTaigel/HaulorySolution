using Haulory.Domain.Enums;

namespace Haulory.Contracts.Vehicles;

public sealed class OdometerReadingRequest
{
    public Guid VehicleAssetId { get; set; }
    public int ReadingKm { get; set; }
    public OdometerReadingType ReadingType { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? RecordedByUserId { get; set; }
    public string? Notes { get; set; }
    public bool UpdateCurrentOdometer { get; set; }
}
