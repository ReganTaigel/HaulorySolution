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
        // ✅ One receipt per job per owner
        var exists = await _db.DeliveryReceipts
            .AnyAsync(r => r.OwnerUserId == receipt.OwnerUserId && r.JobId == receipt.JobId);

        if (exists) return;

        _db.DeliveryReceipts.Add(receipt);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<DeliveryReceipt>> GetByOwnerAsync(Guid ownerUserId)
    {
        return await _db.DeliveryReceipts
            .AsNoTracking()
            .Where(r => r.OwnerUserId == ownerUserId)
            .OrderByDescending(r => r.DeliveredAtUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<DeliveryReceipt>> GetByJobIdAsync(Guid ownerUserId, Guid jobId)
    {
        return await _db.DeliveryReceipts
            .AsNoTracking()
            .Where(r => r.OwnerUserId == ownerUserId && r.JobId == jobId)
            .ToListAsync();
    }

    public async Task<DeliveryReceipt?> GetByIdAsync(Guid ownerUserId, Guid receiptId)
    {
        return await _db.DeliveryReceipts
            .AsNoTracking()
            .FirstOrDefaultAsync(r => r.OwnerUserId == ownerUserId && r.Id == receiptId);
    }

    public async Task<IReadOnlyList<DeliveryReceipt>> GetByOwnerDeliveredBetweenUtcAsync(
        Guid ownerUserId,
        DateTime fromUtc,
        DateTime toUtc)
    {
        if (fromUtc.Kind != DateTimeKind.Utc)
            fromUtc = DateTime.SpecifyKind(fromUtc, DateTimeKind.Utc);

        if (toUtc.Kind != DateTimeKind.Utc)
            toUtc = DateTime.SpecifyKind(toUtc, DateTimeKind.Utc);

        return await _db.DeliveryReceipts
            .AsNoTracking()
            .Where(r => r.OwnerUserId == ownerUserId)
            .Where(r => r.DeliveredAtUtc >= fromUtc && r.DeliveredAtUtc < toUtc)
            .OrderByDescending(r => r.DeliveredAtUtc)
            .ToListAsync();
    }
}