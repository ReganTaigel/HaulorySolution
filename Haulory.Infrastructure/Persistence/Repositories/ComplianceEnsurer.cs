using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Services;

public class ComplianceEnsurer : IComplianceEnsurer
{
    private readonly HauloryDbContext _db;

    public ComplianceEnsurer(HauloryDbContext db)
    {
        _db = db;
    }

    public async Task EnsureAllDriverInductionsExistAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty) return;

        var driverIds = await _db.Drivers
            .Where(d => d.OwnerUserId == ownerUserId)
            .Select(d => d.Id)
            .ToListAsync();

        foreach (var driverId in driverIds)
        {
            await EnsureDriverInductionsExistForDriverAsync(ownerUserId, driverId);
        }
    }

    public async Task EnsureDriverInductionsExistForDriverAsync(Guid ownerUserId, Guid driverId)
    {
        if (ownerUserId == Guid.Empty || driverId == Guid.Empty) return;

        var reqs = await _db.InductionRequirements
            .Where(r => r.OwnerUserId == ownerUserId && r.IsActive)
            .Select(r => new { r.Id, r.WorkSiteId })
            .ToListAsync();

        if (reqs.Count == 0) return;

        var existing = await _db.DriverInductions
            .Where(x => x.OwnerUserId == ownerUserId && x.DriverId == driverId)
            .Select(x => new { x.WorkSiteId, x.RequirementId })
            .ToListAsync();

        var existingSet = existing.Select(x => (x.WorkSiteId, x.RequirementId)).ToHashSet();

        foreach (var req in reqs)
        {
            if (existingSet.Contains((req.WorkSiteId, req.Id)))
                continue;

            _db.DriverInductions.Add(new DriverInduction(
                ownerUserId: ownerUserId,
                driverId: driverId,
                workSiteId: req.WorkSiteId,
                requirementId: req.Id,
                issueDateUtc: DateTime.UtcNow
            ));
        }

        await _db.SaveChangesAsync();
    }

    // ✅ NEW: ensure for ONE driver + ONE site, with issue date
    public async Task EnsureDriverSiteInductionsExistAsync(
        Guid ownerUserId,
        Guid driverId,
        Guid workSiteId,
        DateTime issueDateUtc)
    {
        if (ownerUserId == Guid.Empty || driverId == Guid.Empty || workSiteId == Guid.Empty) return;

        issueDateUtc = DateTime.SpecifyKind(issueDateUtc, DateTimeKind.Utc);

        // Active requirements for this site
        var reqIds = await _db.InductionRequirements
            .Where(r => r.OwnerUserId == ownerUserId && r.IsActive && r.WorkSiteId == workSiteId)
            .Select(r => r.Id)
            .ToListAsync();

        if (reqIds.Count == 0) return;

        // Existing inductions for this driver + site
        var existing = await _db.DriverInductions
            .Where(x => x.OwnerUserId == ownerUserId
                     && x.DriverId == driverId
                     && x.WorkSiteId == workSiteId)
            .ToListAsync();

        var existingReqSet = existing.Select(x => x.RequirementId).ToHashSet();

        // Optional: backfill IssueDateUtc on old rows (default = 0001-01-01)
        foreach (var row in existing)
        {
            if (row.IssueDateUtc == default)
                row.SetIssueDateUtc(issueDateUtc);
        }

        // Create missing rows
        foreach (var reqId in reqIds)
        {
            if (existingReqSet.Contains(reqId))
                continue;

            _db.DriverInductions.Add(new DriverInduction(
                ownerUserId: ownerUserId,
                driverId: driverId,
                workSiteId: workSiteId,
                requirementId: reqId,
                issueDateUtc: issueDateUtc
            ));
        }

        await _db.SaveChangesAsync();
    }
}
