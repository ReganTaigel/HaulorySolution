using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class DeliveryReceiptRepository : IDeliveryReceiptRepository
{
    private readonly HauloryDbContext _db;

    public DeliveryReceiptRepository(HauloryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(DeliveryReceipt receipt)
    {
        // Match JSON behavior: ignore duplicates for the same JobId
        var exists = await _db.DeliveryReceipts
            .AnyAsync(r => r.JobId == receipt.JobId);

        if (exists)
            return;

        _db.DeliveryReceipts.Add(receipt);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<DeliveryReceipt>> GetAllAsync()
    {
        return await _db.DeliveryReceipts
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IReadOnlyList<DeliveryReceipt>> GetByJobIdAsync(Guid jobId)
    {
        return await _db.DeliveryReceipts
            .AsNoTracking()
            .Where(r => r.JobId == jobId)
            .ToListAsync();
    }
}
