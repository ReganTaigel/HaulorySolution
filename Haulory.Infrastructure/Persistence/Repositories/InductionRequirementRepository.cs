using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class InductionRequirementRepository : IInductionRequirementRepository
{
    private readonly HauloryDbContext _db;

    public InductionRequirementRepository(HauloryDbContext db) => _db = db;

    public async Task AddAsync(InductionRequirement req)
    {
        _db.InductionRequirements.Add(req);
        await _db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<InductionRequirement>> GetActiveByOwnerAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty) return Array.Empty<InductionRequirement>();

        return await _db.InductionRequirements
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.IsActive)
            .OrderBy(x => x.WorkSiteId)
            .ThenBy(x => x.Title)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<InductionRequirement>> GetActiveBySiteAsync(Guid ownerUserId, Guid workSiteId)
    {
        if (ownerUserId == Guid.Empty || workSiteId == Guid.Empty) return Array.Empty<InductionRequirement>();

        return await _db.InductionRequirements
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId
                     && x.WorkSiteId == workSiteId
                     && x.IsActive)
            .OrderBy(x => x.Title)
            .ToListAsync();
    }
}
