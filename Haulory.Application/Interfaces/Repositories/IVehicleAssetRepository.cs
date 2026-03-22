using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

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

    // NEW: batch trailer validation / lookup
    Task<IReadOnlyList<VehicleAsset>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<IReadOnlyList<VehicleAsset>> GetTrailerAssetsByOwnerAsync(Guid ownerUserId);

    // ✅ NEW: sub-user vehicles from active assigned jobs (truck + trailers)
    Task<IReadOnlyList<VehicleAsset>> GetActiveAssetsAssignedToUserAsync(Guid ownerUserId, Guid assignedToUserId);

    // NEW: Plan limit counts
    Task<int> CountPoweredUnitsAsync(Guid ownerUserId);
    Task<int> CountTrailersAsync(Guid ownerUserId);

    // NEW: Prevent duplicate rego per owner (optional but recommended)
    Task<bool> RegoExistsAsync(Guid ownerUserId, string rego, Guid? excludeAssetId = null);
}