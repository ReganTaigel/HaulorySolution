using System.Text.Json;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

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
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "vehicleAssets.json");
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
            var assets = await LoadUnsafeAsync();

            foreach (var a in assetsToAdd)
                assets.Add(Normalize(a));

            await SaveUnsafeAtomicAsync(assets);
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
            await SaveUnsafeAtomicAsync(assets);
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
                await SaveUnsafeAtomicAsync(assets);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<VehicleAsset>> LoadUnsafeAsync()
    {
        if (!File.Exists(_filePath))
            return new List<VehicleAsset>();

        try
        {
            var json = await File.ReadAllTextAsync(_filePath);

            // tolerate empty file
            if (string.IsNullOrWhiteSpace(json))
                return new List<VehicleAsset>();

            return JsonSerializer.Deserialize<List<VehicleAsset>>(json, JsonOptions)
                   ?? new List<VehicleAsset>();
        }
        catch (JsonException)
        {
            // If corrupted, preserve it for diagnostics and start clean
            var corruptPath = _filePath + ".corrupt_" + DateTime.UtcNow.ToString("yyyyMMddHHmmss");
            try { File.Copy(_filePath, corruptPath, overwrite: true); } catch { /* ignore */ }

            return new List<VehicleAsset>();
        }
    }

    private async Task SaveUnsafeAtomicAsync(List<VehicleAsset> assets)
    {
        var json = JsonSerializer.Serialize(assets, JsonOptions);

        var dir = Path.GetDirectoryName(_filePath)!;
        Directory.CreateDirectory(dir);

        var tmp = _filePath + ".tmp";
        await File.WriteAllTextAsync(tmp, json);

        // atomic-ish replace
        if (File.Exists(_filePath))
            File.Delete(_filePath);

        File.Move(tmp, _filePath);
    }

    private static VehicleAsset Normalize(VehicleAsset a)
    {
        // basic hygiene (doesn't change business meaning)
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
