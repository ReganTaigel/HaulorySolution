using System.Text.Json;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Infrastructure.Persistence.Json;

public class VehicleAssetRepository : IVehicleAssetRepository
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public VehicleAssetRepository()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "vehicleAssets.json");
    }

    public async Task AddAsync(VehicleAsset asset)
    {
        await _lock.WaitAsync();
        try
        {
            var assets = await LoadAsync();
            assets.Add(asset);
            await SaveAsync(assets);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task AddRangeAsync(IReadOnlyList<VehicleAsset> assetsToAdd)
    {
        await _lock.WaitAsync();
        try
        {
            var assets = await LoadAsync();
            assets.AddRange(assetsToAdd);
            await SaveAsync(assets);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IReadOnlyList<VehicleAsset>> GetAllAsync()
    {
        return await LoadAsync();
    }

    public async Task<VehicleAsset?> GetByIdAsync(Guid id)
    {
        var assets = await LoadAsync();
        return assets.FirstOrDefault(a => a.Id == id);
    }

    public async Task UpdateAsync(VehicleAsset asset)
    {
        await _lock.WaitAsync();
        try
        {
            var assets = await LoadAsync();
            var index = assets.FindIndex(a => a.Id == asset.Id);
            if (index < 0) return;

            assets[index] = asset;
            await SaveAsync(assets);
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
            var assets = await LoadAsync();
            var removed = assets.RemoveAll(a => a.Id == id);
            if (removed > 0)
                await SaveAsync(assets);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<VehicleAsset>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<VehicleAsset>();

        var json = await File.ReadAllTextAsync(_filePath);

        return JsonSerializer.Deserialize<List<VehicleAsset>>(json)
               ?? new List<VehicleAsset>();
    }

    private async Task SaveAsync(List<VehicleAsset> assets)
    {
        var json = JsonSerializer.Serialize(
            assets,
            new JsonSerializerOptions { WriteIndented = true });

        await File.WriteAllTextAsync(_filePath, json);
    }
}
