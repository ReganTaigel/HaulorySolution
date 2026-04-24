using HaulitCore.Application.Services;
using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public class HubodometerServiceRepository : IHubodometerService
{
    private readonly HaulitCoreDbContext _dbContext;

    public HubodometerServiceRepository(HaulitCoreDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task RecordReadingAsync(
        Guid vehicleAssetId,
        int readingKm,
        HubodometerReadingType readingType,
        Guid? driverId,
        Guid? recordedByUserId,
        string? notes,
        bool updateCurrentOdometer = true,
        CancellationToken cancellationToken = default)
    {
        if (vehicleAssetId == Guid.Empty)
            throw new ArgumentException("Vehicle asset id is required.", nameof(vehicleAssetId));

        if (readingKm < 0)
            throw new ArgumentOutOfRangeException(nameof(readingKm), "Reading cannot be negative.");

        if (driverId == Guid.Empty)
            driverId = null;

        if (recordedByUserId == Guid.Empty)
            recordedByUserId = null;

        var asset = await _dbContext.VehicleAssets
            .FirstOrDefaultAsync(x => x.Id == vehicleAssetId, cancellationToken);

        if (asset == null)
            throw new InvalidOperationException("Vehicle asset not found.");

        if (driverId.HasValue)
        {
            var driverExists = await _dbContext.Drivers
                .AnyAsync(x => x.Id == driverId.Value, cancellationToken);

            if (!driverExists)
                throw new InvalidOperationException("Selected driver does not exist.");
        }

        if (recordedByUserId.HasValue)
        {
            var userExists = await _dbContext.UserAccounts
                .AnyAsync(x => x.Id == recordedByUserId.Value, cancellationToken);

            if (!userExists)
                throw new InvalidOperationException("Recorded-by user does not exist.");
        }

        var allowDecrease =
            readingType == HubodometerReadingType.ManualAdjustment ||
            readingType == HubodometerReadingType.ComplianceCorrection;

        if (!allowDecrease &&
            asset.HubodometerKm.HasValue &&
            readingKm < asset.HubodometerKm.Value)
        {
            throw new InvalidOperationException("Reading cannot be less than the current odometer.");
        }

        var reading = new HubodometerReading(
            vehicleAssetId: asset.Id,
            unitNumber: asset.UnitNumber,
            readingKm: readingKm,
            readingType: readingType,
            driverId: driverId,
            recordedByUserId: recordedByUserId,
            notes: notes);

        _dbContext.HubodometerReadings.Add(reading);

        if (updateCurrentOdometer)
        {
            asset.SetOdometer(readingKm, allowDecrease);
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task AdjustHubodometerAsync(
        Guid vehicleAssetId,
        int correctedKm,
        Guid recordedByUserId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        if (vehicleAssetId == Guid.Empty)
            throw new ArgumentException("Vehicle asset id is required.", nameof(vehicleAssetId));

        if (correctedKm < 0)
            throw new ArgumentOutOfRangeException(nameof(correctedKm), "Corrected km cannot be negative.");

        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reason is required.", nameof(reason));

        var asset = await _dbContext.VehicleAssets
            .FirstOrDefaultAsync(x => x.Id == vehicleAssetId, cancellationToken);

        if (asset == null)
            throw new InvalidOperationException("Vehicle asset not found.");

        var reading = new HubodometerReading(
            vehicleAssetId: asset.Id,
            unitNumber: asset.UnitNumber,
            readingKm: correctedKm,
            readingType: HubodometerReadingType.ManualAdjustment,
            driverId: null,
            recordedByUserId: recordedByUserId,
            notes: reason);

        _dbContext.HubodometerReadings.Add(reading);

        asset.SetOdometer(correctedKm, allowDecrease: true);

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<List<HubodometerReading>> GetReadingsAsync(
        Guid vehicleAssetId,
        CancellationToken cancellationToken = default)
    {
        if (vehicleAssetId == Guid.Empty)
            throw new ArgumentException("Vehicle asset id is required.", nameof(vehicleAssetId));

        return await _dbContext.HubodometerReadings
            .Where(x => x.VehicleAssetId == vehicleAssetId)
            .OrderByDescending(x => x.RecordedAtUtc)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<VehicleAsset>> GetAssetsForOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default)
    {
        if (ownerUserId == Guid.Empty)
            return new List<VehicleAsset>();

        return await _dbContext.VehicleAssets
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ToListAsync(cancellationToken);
    }
}