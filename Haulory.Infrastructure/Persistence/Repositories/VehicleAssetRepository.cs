using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class VehicleAssetRepository : IVehicleAssetRepository
{
    #region Dependencies

    private readonly HauloryDbContext _db;

    #endregion

    #region Constructor

    public VehicleAssetRepository(HauloryDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Commands

    public async Task AddAsync(VehicleAsset asset)
    {
        await AddRangeAsync(new[] { asset });
    }

    public async Task AddRangeAsync(IReadOnlyList<VehicleAsset> assetsToAdd)
    {
        if (assetsToAdd == null || assetsToAdd.Count == 0)
            return;

        foreach (var incoming in assetsToAdd)
        {
            var normalized = Normalize(incoming);

            // 1) Match by Id
            var existingById = await _db.VehicleAssets
                .FirstOrDefaultAsync(a => a.Id == normalized.Id && normalized.Id != Guid.Empty);

            if (existingById != null)
            {
                _db.Entry(existingById).CurrentValues.SetValues(normalized);
                continue;
            }

            // 2) Match by VehicleSetId + UnitNumber
            if (normalized.VehicleSetId != Guid.Empty && normalized.UnitNumber > 0)
            {
                var existingBySlot = await _db.VehicleAssets
                    .FirstOrDefaultAsync(a =>
                        a.VehicleSetId == normalized.VehicleSetId &&
                        a.UnitNumber == normalized.UnitNumber);

                if (existingBySlot != null)
                {
                    _db.Entry(existingBySlot).CurrentValues.SetValues(normalized);
                    continue;
                }
            }

            // 3) New
            await _db.VehicleAssets.AddAsync(normalized);
        }

        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(VehicleAsset asset)
    {
        var existing = await _db.VehicleAssets.FindAsync(asset.Id);
        if (existing == null)
            return;

        var normalized = Normalize(asset);
        _db.Entry(existing).CurrentValues.SetValues(normalized);

        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var asset = await _db.VehicleAssets.FindAsync(id);
        if (asset == null)
            return;

        _db.VehicleAssets.Remove(asset);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Queries

    public async Task<IReadOnlyList<VehicleAsset>> GetAllAsync()
    {
        return await _db.VehicleAssets
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<VehicleAsset?> GetByIdAsync(Guid id)
    {
        return await _db.VehicleAssets
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.Id == id);
    }

    public async Task<int> CountPoweredUnitsAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return 0;

        return await _db.VehicleAssets
            .AsNoTracking()
            .CountAsync(v =>
                v.OwnerUserId == ownerUserId &&
                v.Kind == AssetKind.PowerUnit);
    }

    public async Task<int> CountTrailersAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return 0;

        return await _db.VehicleAssets
            .AsNoTracking()
            .CountAsync(v =>
                v.OwnerUserId == ownerUserId &&
                v.Kind == AssetKind.Trailer);
    }

    public async Task<bool> RegoExistsAsync(Guid ownerUserId, string rego, Guid? excludeAssetId = null)
    {
        if (ownerUserId == Guid.Empty)
            return false;

        var normRego = (rego ?? string.Empty).Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(normRego))
            return false;

        return await _db.VehicleAssets
            .AsNoTracking()
            .AnyAsync(v =>
                v.OwnerUserId == ownerUserId &&
                v.Rego.ToUpper() == normRego &&
                (!excludeAssetId.HasValue || v.Id != excludeAssetId.Value));
    }

    #endregion

    #region Normalization

    private static VehicleAsset Normalize(VehicleAsset a)
    {
        // Normalize strings
        a.Rego = (a.Rego ?? string.Empty).Trim().ToUpperInvariant();
        a.Make = (a.Make ?? string.Empty).Trim();
        a.Model = (a.Model ?? string.Empty).Trim();

        // Ensure ids exist
        if (a.VehicleSetId == Guid.Empty)
            a.VehicleSetId = Guid.NewGuid();

        if (a.Id == Guid.Empty)
            a.Id = Guid.NewGuid();

        // Ensure created timestamp exists
        if (a.CreatedUtc == default)
            a.CreatedUtc = DateTime.UtcNow;

        return a;
    }

    #endregion
}