using HaulitCore.Domain.Enums;

namespace HaulitCore.Contracts.Vehicles;

public sealed class HubodometerReadingRequest
{
    public Guid VehicleAssetId { get; set; }
    public int ReadingKm { get; set; }
    public HubodometerReadingType ReadingType { get; set; }
    public Guid? DriverId { get; set; }
    public Guid? RecordedByUserId { get; set; }
    public string? Notes { get; set; }
    public bool UpdateCurrentHubodometer { get; set; }
}
