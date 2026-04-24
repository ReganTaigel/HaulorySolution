using HaulitCore.Domain.Enums;

namespace HaulitCore.Application.Odometering.Models;

public class RecordHubodometerReadingRequest
{
    public Guid VehicleAssetId { get; set; }
    public int UnitNumber { get; set; }
    public int ReadingKm { get; set; }
    public HubodometerReadingType ReadingType { get; set; }
    public Guid? DriverId { get; set; }
    public string? Notes { get; set; }

    // Usually true for end-of-day or trusted readings
    public bool UpdateCurrentHubodometer { get; set; } = true;
}