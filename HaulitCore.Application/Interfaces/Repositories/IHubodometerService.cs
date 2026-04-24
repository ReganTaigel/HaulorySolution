using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;

namespace HaulitCore.Application.Services;

public interface IHubodometerService
{
    Task RecordReadingAsync(
        Guid vehicleAssetId,
        int readingKm,
        HubodometerReadingType readingType,
        Guid? driverId,
        Guid? recordedByUserId,
        string? notes,
        bool updateCurrentHubodometer = true,
        CancellationToken cancellationToken = default);

    Task AdjustHubodometerAsync(
        Guid vehicleAssetId,
        int correctedKm,
        Guid recordedByUserId,
        string reason,
        CancellationToken cancellationToken = default);

    Task<List<HubodometerReading>> GetReadingsAsync(
        Guid vehicleAssetId,
        CancellationToken cancellationToken = default);

    Task<List<VehicleAsset>> GetAssetsForOwnerAsync(
        Guid ownerUserId,
        CancellationToken cancellationToken = default);
}