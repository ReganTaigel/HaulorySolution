using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Interfaces.Repositories;

public interface IVehicleAssetRepository
{
    // Commands
    Task AddAsync(VehicleAsset asset);
    Task AddRangeAsync(IReadOnlyList<VehicleAsset> assetsToAdd);
    Task UpdateAsync(VehicleAsset vehicle, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<VehicleAsset>> GetByOwnerAsync(Guid ownerUserId);

    // Queries
    Task<IReadOnlyList<VehicleAsset>> GetAllAsync();
    Task<VehicleAsset?> GetByIdAsync(Guid id);

    //  batch trailer validation / lookup
    Task<IReadOnlyList<VehicleAsset>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<IReadOnlyList<VehicleAsset>> GetTrailerAssetsByOwnerAsync(Guid ownerUserId);

    //  sub-user vehicles from active assigned jobs (truck + trailers)
    Task<IReadOnlyList<VehicleAsset>> GetActiveAssetsAssignedToUserAsync(Guid ownerUserId, Guid assignedToUserId);

    //  Plan limit counts
    Task<int> CountPoweredUnitsAsync(Guid ownerUserId);
    Task<int> CountTrailersAsync(Guid ownerUserId);

    // Prevent duplicate rego per owner (optional but recommended)
    Task<bool> RegoExistsAsync(Guid ownerUserId, string rego, Guid? excludeAssetId = null);

    // Needed so edit can load all units belonging to the same set.
    Task<IReadOnlyList<VehicleAsset>> GetByVehicleSetIdAsync(Guid vehicleSetId, CancellationToken cancellationToken = default);

    // Useful for resolving the set from any one asset id during edit/update.
    Task<Guid?> GetVehicleSetIdByAssetIdAsync(Guid assetId, CancellationToken cancellationToken = default);
}