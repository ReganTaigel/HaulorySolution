using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly HauloryDbContext _db;

    public JobRepository(HauloryDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Job job)
    {
        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<Job>> GetAllAsync()
    {
        // JSON returned in file order; DB has no inherent order, so we order by SortOrder
        return await _db.Jobs
            .AsNoTracking()
            .OrderBy(j => j.SortOrder)
            .ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        return await _db.Jobs
            .AsNoTracking()
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task UpdateAsync(Job job)
    {
        _db.Jobs.Update(job);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var job = await _db.Jobs.FindAsync(id);
        if (job == null) return;

        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync();
    }

    public async Task<int> GetNextSortOrderAsync()
    {
        var max = await _db.Jobs.MaxAsync(j => (int?)j.SortOrder);
        return (max ?? 0) + 1;
    }

    public async Task UpdateAllAsync(IReadOnlyList<Job> jobs)
    {
        // Matches your JSON SaveAsync(list) semantics: overwrite the set
        await using var tx = await _db.Database.BeginTransactionAsync();

        _db.Jobs.RemoveRange(_db.Jobs);
        await _db.SaveChangesAsync();

        _db.Jobs.AddRange(jobs);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();
    }
}
