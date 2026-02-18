using System;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    #region Dependencies

    private readonly HauloryDbContext _db;

    #endregion

    #region Constructor

    public JobRepository(HauloryDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Commands

    public async Task AddAsync(Job job)
    {
        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Job job)
    {
        _db.Jobs.Update(job);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var job = await _db.Jobs.FindAsync(id);
        if (job == null)
            return;

        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAllAsync(IReadOnlyList<Job> jobs)
    {
        // Matches previous JSON SaveAsync(list) semantics:
        // Overwrite the entire job set
        await using var tx = await _db.Database.BeginTransactionAsync();

        _db.Jobs.RemoveRange(_db.Jobs);
        await _db.SaveChangesAsync();

        _db.Jobs.AddRange(jobs);
        await _db.SaveChangesAsync();

        await tx.CommitAsync();
    }

    #endregion

    #region Queries

    public async Task<IReadOnlyList<Job>> GetAllAsync()
    {
        // DB has no inherent order; enforce SortOrder
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

    public async Task<int> GetNextSortOrderAsync()
    {
        var max = await _db.Jobs.MaxAsync(j => (int?)j.SortOrder);
        return (max ?? 0) + 1;
    }

    #endregion
}
