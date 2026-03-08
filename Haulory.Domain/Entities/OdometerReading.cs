using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class OdometerReading
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid VehicleAssetId { get; private set; }

    // 1 = Power unit
    // 2 = Trailer 1
    // 3 = Trailer 2
    public int UnitNumber { get; private set; }

    public DateTime RecordedAtUtc { get; private set; } = DateTime.UtcNow;

    public int ReadingKm { get; private set; }

    public OdometerReadingType ReadingType { get; private set; }

    public Guid? DriverId { get; private set; }
    public Guid? RecordedByUserId { get; private set; }

    public string? Notes { get; private set; }

    private OdometerReading() { } // EF

    public OdometerReading(
        Guid vehicleAssetId,
        int unitNumber,
        int readingKm,
        OdometerReadingType readingType,
        Guid? driverId,
        Guid? recordedByUserId,
        string? notes)
    {
        if (vehicleAssetId == Guid.Empty)
            throw new ArgumentException("VehicleAssetId required.", nameof(vehicleAssetId));

        if (unitNumber < 1)
            throw new ArgumentOutOfRangeException(nameof(unitNumber), "UnitNumber must be >= 1.");

        if (readingKm < 0)
            throw new ArgumentOutOfRangeException(nameof(readingKm), "Reading cannot be negative.");

        VehicleAssetId = vehicleAssetId;
        UnitNumber = unitNumber;
        ReadingKm = readingKm;
        ReadingType = readingType;
        DriverId = driverId;
        RecordedByUserId = recordedByUserId;
        Notes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
    }
}