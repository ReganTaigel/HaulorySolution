using Haulory.Domain.Enums;

namespace Haulory.Application.Odometering.Models;

public class RecordOdometerReadingRequest
{
    public Guid VehicleAssetId { get; set; }
    public int UnitNumber { get; set; }
    public int ReadingKm { get; set; }
    public OdometerReadingType ReadingType { get; set; }
    public Guid? DriverId { get; set; }
    public string? Notes { get; set; }

    // Usually true for end-of-day or trusted readings
    public bool UpdateCurrentOdometer { get; set; } = true;
}