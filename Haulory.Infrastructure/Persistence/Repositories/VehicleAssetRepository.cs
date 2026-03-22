using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public sealed class VehicleAssetRepository : IVehicleAssetRepository
{
    private readonly HauloryDbContext _db;

    public VehicleAssetRepository(HauloryDbContext db)
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
            .Where(x => x.OwnerUserId == ownerUserId)
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
            .Where(x => x.OwnerUserId == ownerUserId)
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
}