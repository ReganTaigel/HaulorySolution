using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Storage;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace Haulory.Infrastructure.Repositories
{
    public class DriverRepository : IDriverRepository
    {
        private readonly string _filePath;

        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        private readonly SemaphoreSlim _gate = new(1, 1);

        public DriverRepository()
        {
            _filePath = Path.Combine(FileSystem.AppDataDirectory, "drivers.json.enc");
        }

        // ✅ Add this
        public string DebugFilePath => _filePath;

        private async Task<List<Driver>> LoadAllUnsafeAsync()
        {
            return await EncryptedJsonStore.LoadAsync<List<Driver>>(_filePath, JsonOptions)
                   ?? new List<Driver>();
        }

        public async Task<List<Driver>> GetAllAsync()
        {
            await _gate.WaitAsync();
            try
            {
                return await LoadAllUnsafeAsync();
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<List<Driver>> GetAllByOwnerUserIdAsync(Guid ownerUserId)
        {
            if (ownerUserId == Guid.Empty)
                return new List<Driver>();

            await _gate.WaitAsync();
            try
            {
                var all = await LoadAllUnsafeAsync();
                return all.Where(d => d.OwnerUserId == ownerUserId).ToList();
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task<Driver?> GetByUserIdAsync(Guid userId)
        {
            if (userId == Guid.Empty)
                return null;

            await _gate.WaitAsync();
            try
            {
                var all = await LoadAllUnsafeAsync();
                return all.FirstOrDefault(d => d.UserId.HasValue && d.UserId.Value == userId);
            }
            finally
            {
                _gate.Release();
            }
        }

        public async Task SaveAsync(Driver driver)
        {
            if (driver == null)
                throw new ArgumentNullException(nameof(driver));

            if (driver.OwnerUserId == Guid.Empty)
                throw new InvalidOperationException("Driver.OwnerUserId must be set before saving.");

            await _gate.WaitAsync();
            try
            {
                var all = await LoadAllUnsafeAsync();

                var idx = all.FindIndex(d => d.Id == driver.Id);
                if (idx >= 0) all[idx] = driver;
                else all.Add(driver);

                await EncryptedJsonStore.SaveAsync(_filePath, all, JsonOptions);
            }
            finally
            {
                _gate.Release();
            }
        }
    }
}
