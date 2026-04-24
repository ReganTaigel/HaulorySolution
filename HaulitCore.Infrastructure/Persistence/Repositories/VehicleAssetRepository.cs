using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public sealed class VehicleAssetRepository : IVehicleAssetRepository
{
    private readonly HaulitCoreDbContext _db;

    public VehicleAssetRepository(HaulitCoreDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(VehicleAsset asset)
    {
        _db.VehicleAssets.Add(asset);
        await _db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IReadOnlyList<VehicleAsset> assetsToAdd)
    {
        _db.VehicleAssets.AddRange(assetsToAdd);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(VehicleAsset vehicle, CancellationToken cancellationToken = default)
    {
        _db.VehicleAssets.Update(vehicle);
        await _db.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var asset = await _db.VehicleAssets
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (asset == null)
            return false;

        _db.VehicleAssets.Remove(asset);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }

    public async Task<IReadOnlyList<VehicleAsset>> GetAllAsync()
    {
        return await _db.VehicleAssets
            .Where(x => x.VehicleSetId != Guid.Empty) // CHANGED
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync();
    }

    public async Task<VehicleAsset?> GetByIdAsync(Guid id)
    {
        return await _db.VehicleAssets.FirstOrDefaultAsync(x => x.Id == id);
    }

    public async Task<IReadOnlyList<VehicleAsset>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var idList = ids
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (idList.Count == 0)
            return Array.Empty<VehicleAsset>();

        return await _db.VehicleAssets
            .Where(x => idList.Contains(x.Id))
            .ToListAsync();
    }

    public async Task<IReadOnlyList<VehicleAsset>> GetByOwnerAsync(Guid ownerUserId)
    {
        return await _db.VehicleAssets
            .Where(x =>
                x.OwnerUserId == ownerUserId &&
                x.VehicleSetId != Guid.Empty) //  only active set members in main vehicle list
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<VehicleAsset>> GetTrailerAssetsByOwnerAsync(Guid ownerUserId)
    {
        return await _db.VehicleAssets
            .Where(x => x.OwnerUserId == ownerUserId && x.Kind == AssetKind.Trailer)
            .OrderBy(x => x.Rego)
            .ThenBy(x => x.UnitNumber)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<VehicleAsset>> GetActiveAssetsAssignedToUserAsync(Guid ownerUserId, Guid assignedToUserId)
    {
        return await _db.VehicleAssets
            .Where(x =>
                x.OwnerUserId == ownerUserId &&
                x.VehicleSetId != Guid.Empty) // exclude detached assets
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync();
    }

    public async Task<int> CountPoweredUnitsAsync(Guid ownerUserId)
    {
        return await _db.VehicleAssets
            .CountAsync(x => x.OwnerUserId == ownerUserId && x.Kind == AssetKind.PowerUnit);
    }

    public async Task<int> CountTrailersAsync(Guid ownerUserId)
    {
        return await _db.VehicleAssets
            .CountAsync(x => x.OwnerUserId == ownerUserId && x.Kind == AssetKind.Trailer);
    }

    public async Task<bool> RegoExistsAsync(Guid ownerUserId, string rego, Guid? excludeAssetId = null)
    {
        rego = rego.Trim();

        return await _db.VehicleAssets.AnyAsync(x =>
            x.OwnerUserId == ownerUserId &&
            x.Rego == rego &&
            (!excludeAssetId.HasValue || x.Id != excludeAssetId.Value));
    }
    // CHANGED:
    // Loads all assets belonging to the same vehicle set.
    // Required for edit (multi-unit) scenarios.
    public async Task<IReadOnlyList<VehicleAsset>> GetByVehicleSetIdAsync(
        Guid vehicleSetId,
        CancellationToken cancellationToken = default)
    {
        return await _db.VehicleAssets
            .Where(x => x.VehicleSetId == vehicleSetId)
            .OrderBy(x => x.UnitNumber)
            .ThenBy(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);
    }

    // CHANGED:
    // Resolves VehicleSetId from a single asset id.
    // Used when edit starts from one asset.
    public async Task<Guid?> GetVehicleSetIdByAssetIdAsync(
        Guid assetId,
        CancellationToken cancellationToken = default)
    {
        return await _db.VehicleAssets
            .Where(x => x.Id == assetId)
            .Select(x => (Guid?)x.VehicleSetId)
            .FirstOrDefaultAsync(cancellationToken);
    }
}