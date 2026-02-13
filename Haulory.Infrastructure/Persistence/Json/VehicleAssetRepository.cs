using System.Text.Json;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Storage;

namespace Haulory.Infrastructure.Persistence.Json;

public class VehicleAssetRepository : IVehicleAssetRepository
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public VehicleAssetRepository()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "vehicleAssets.json.enc");
    }

    public async Task AddAsync(VehicleAsset asset)
    {
        await AddRangeAsync(new[] { asset });
    }

    public async Task AddRangeAsync(IReadOnlyList<VehicleAsset> assetsToAdd)
    {
        if (assetsToAdd == null || assetsToAdd.Count == 0) return;

        await _lock.WaitAsync();
        try
        {
            var existing = await LoadUnsafeAsync();

            foreach (var incoming in assetsToAdd)
            {
                var normalized = Normalize(incoming);

                // 1) Match by Id
                var indexById = existing.FindIndex(a => a.Id == normalized.Id && normalized.Id != Guid.Empty);
                if (indexById >= 0)
                {
                    existing[indexById] = normalized;
                    continue;
                }

                // 2) Match by VehicleSetId + UnitNumber
                if (normalized.VehicleSetId != Guid.Empty && normalized.UnitNumber > 0)
                {
                    var indexBySlot = existing.FindIndex(a =>
                        a.VehicleSetId == normalized.VehicleSetId &&
                        a.UnitNumber == normalized.UnitNumber);

                    if (indexBySlot >= 0)
                    {
                        existing[indexBySlot] = normalized;
                        continue;
                    }
                }

                // 3) New
                existing.Add(normalized);
            }

            await SaveUnsafeAsync(existing);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<VehicleAsset>> GetAllAsync()
    {
        await _lock.WaitAsync();
        try
        {
            return await LoadUnsafeAsync();
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<VehicleAsset?> GetByIdAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var assets = await LoadUnsafeAsync();
            return assets.FirstOrDefault(a => a.Id == id);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateAsync(VehicleAsset asset)
    {
        await _lock.WaitAsync();
        try
        {
            var assets = await LoadUnsafeAsync();
            var index = assets.FindIndex(a => a.Id == asset.Id);
            if (index < 0) return;

            assets[index] = Normalize(asset);
            await SaveUnsafeAsync(assets);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _lock.WaitAsync();
        try
        {
            var assets = await LoadUnsafeAsync();
            var removed = assets.RemoveAll(a => a.Id == id);

            if (removed > 0)
                await SaveUnsafeAsync(assets);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<VehicleAsset>> LoadUnsafeAsync()
    {
        var data = await EncryptedJsonStore.LoadAsync<List<VehicleAsset>>(_filePath, JsonOptions);
        return data ?? new List<VehicleAsset>();
    }

    private async Task SaveUnsafeAsync(List<VehicleAsset> assets)
    {
        await EncryptedJsonStore.SaveAsync(_filePath, assets, JsonOptions);
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
