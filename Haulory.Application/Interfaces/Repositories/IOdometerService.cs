using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Services;

public interface IOdometerService
{
    Task RecordReadingAsync(
        Guid vehicleAssetId,
        int readingKm,
        OdometerReadingType readingType,
        Guid? driverId,
        Guid? recordedByUserId,
        string? notes,
        bool updateCurrentOdometer = true,
        CancellationToken cancellationToken = default);

    Task AdjustOdometerAsync(
        Guid vehicleAssetId,
        int correctedKm,
        Guid recordedByUserId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<List<OdometerReading>> GetReadingsAsync(
        Guid vehicleAssetId,
        CancellationToken cancellationToken = default);

    Task<List<VehicleAsset>> GetAssetsForOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);
}