using HaulitCore.Domain.Enums;

namespace HaulitCore.Domain.Entities;

public class HubodometerReading
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public Guid VehicleAssetId { get; private set; }

    // 1 = Power unit
    // 2 = Trailer 1
    // 3 = Trailer 2
    public int UnitNumber { get; private set; }

    public DateTime RecordedAtUtc { get; private set; } = DateTime.UtcNow;

    public int ReadingKm { get; private set; }

    public HubodometerReadingType ReadingType { get; private set; }

    public Guid? DriverId { get; private set; }
    public Guid? RecordedByUserId { get; private set; }

    public string? Notes { get; private set; }

    private HubodometerReading() { } // EF

    public HubodometerReading(
        Guid vehicleAssetId,
        int unitNumber,
        int readingKm,
        HubodometerReadingType readingType,
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