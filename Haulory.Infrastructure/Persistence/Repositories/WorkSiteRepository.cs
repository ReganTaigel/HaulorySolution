using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class WorkSiteRepository : IWorkSiteRepository
{
    private readonly HauloryDbContext _db;

    public WorkSiteRepository(HauloryDbContext db) => _db = db;

    public async Task AddAsync(WorkSite site)
    {
        _db.WorkSites.Add(site);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<WorkSite>> GetAllByOwnerAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty) return Array.Empty<WorkSite>();

        return await _db.WorkSites
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.IsActive)
            .OrderBy(x => x.Name)
            .ToListAsync();
    }

    public async Task<WorkSite?> GetByIdAsync(Guid ownerUserId, Guid workSiteId)
    {
        if (ownerUserId == Guid.Empty || workSiteId == Guid.Empty) return null;

        return await _db.WorkSites
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.OwnerUserId == ownerUserId && x.Id == workSiteId);
    }
}
