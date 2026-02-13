using System.Text.Json;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Storage;

namespace Haulory.Infrastructure.Persistence.Json;

public class DeliveryReceiptRepository : IDeliveryReceiptRepository
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public DeliveryReceiptRepository()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "delivery_receipts.json.enc");
    }

    public async Task AddAsync(DeliveryReceipt receipt)
    {
        await _lock.WaitAsync();
        try
        {
            var receipts = await LoadAsync();

            if (receipts.Any(r => r.JobId == receipt.JobId))
                return;

            receipts.Add(receipt);
            await SaveAsync(receipts);
        }
        finally { _lock.Release(); }
    }

    public async Task<IReadOnlyList<DeliveryReceipt>> GetAllAsync()
        => await LoadAsync();

    public async Task<IReadOnlyList<DeliveryReceipt>> GetByJobIdAsync(Guid jobId)
    {
        var all = await LoadAsync();
        return all.Where(r => r.JobId == jobId).ToList();
    }

    private async Task<List<DeliveryReceipt>> LoadAsync()
    {
        var data = await EncryptedJsonStore.LoadAsync<List<DeliveryReceipt>>(_filePath, JsonOptions);
        return data ?? new List<DeliveryReceipt>();
    }

    private async Task SaveAsync(List<DeliveryReceipt> receipts)
    {
        await EncryptedJsonStore.SaveAsync(_filePath, receipts, JsonOptions);
    }
}
