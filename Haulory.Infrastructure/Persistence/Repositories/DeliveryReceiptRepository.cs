using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class DeliveryReceiptRepository : IDeliveryReceiptRepository
{
    #region Dependencies

    private readonly HauloryDbContext _db;

    #endregion

    #region Constructor

    public DeliveryReceiptRepository(HauloryDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Commands

    public async Task AddAsync(DeliveryReceipt receipt)
    {
        // Match previous JSON behavior:
        // Ignore duplicates for the same JobId (1 receipt per job)
        var exists = await _db.DeliveryReceipts
            .AnyAsync(r => r.JobId == receipt.JobId);

        if (exists)
            return;

        _db.DeliveryReceipts.Add(receipt);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Queries

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

    #endregion
}
