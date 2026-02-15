using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class VehicleAssetRepository : IVehicleAssetRepository
{
    private readonly HauloryDbContext _db;

    public VehicleAssetRepository(HauloryDbContext db)
    {
        _db = db;
    }

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

            // 1️⃣ Match by Id
            var existingById = await _db.VehicleAssets
                .FirstOrDefaultAsync(a => a.Id == normalized.Id && normalized.Id != Guid.Empty);

            if (existingById != null)
            {
                _db.Entry(existingById).CurrentValues.SetValues(normalized);
                continue;
            }

            // 2️⃣ Match by VehicleSetId + UnitNumber
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

            // 3️⃣ New
            await _db.VehicleAssets.AddAsync(normalized);
        }

        await _db.SaveChangesAsync();
    }

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

    private static VehicleAsset Normalize(VehicleAsset a)
    {
        a.Rego = (a.Rego ?? string.Empty).Trim().ToUpperInvariant();
        a.Make = (a.Make ?? string.Empty).Trim();
        a.Model = (a.Model ?? string.Empty).Trim();

        if (a.VehicleSetId == Guid.Empty)
            a.VehicleSetId = Guid.NewGuid();

        if (a.Id == Guid.Empty)
            a.Id = Guid.NewGuid();

        if (a.CreatedUtc == default)
            a.CreatedUtc = DateTime.UtcNow;

        return a;
    }
}
