using System;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public class WorkSiteRepository : IWorkSiteRepository
{
    #region Dependencies

    private readonly HaulitCoreDbContext _db;

    #endregion

    #region Constructor

    public WorkSiteRepository(HaulitCoreDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Commands

    public async Task AddAsync(WorkSite site)
    {
        _db.WorkSites.Add(site);
        await _db.SaveChangesAsync();
    }

    #endregion

    #region Queries

    public async Task<IReadOnlyList<WorkSite>> GetAllByOwnerAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return Array.Empty<WorkSite>();

        return await _db.WorkSites
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<WorkSite?> GetByIdAsync(Guid ownerUserId, Guid workSiteId)
    {
        if (ownerUserId == Guid.Empty || workSiteId == Guid.Empty)
            return null;

        return await _db.WorkSites
            .AsNoTracking()
            .FirstOrDefaultAsync(x =>
                x.OwnerUserId == ownerUserId &&
                x.Id == workSiteId);
    }

    #endregion
}
