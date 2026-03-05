using System;
using Haulory.Application.Features.Incductions;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class DriverInductionRepository : IDriverInductionRepository
{
    #region Dependencies

    private readonly HauloryDbContext _db;

    #endregion

    #region Constructor

    public DriverInductionRepository(HauloryDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Commands

    public async Task AddAsync(DriverInduction record)
    {
        _db.DriverInductions.Add(record);
        await _db.SaveChangesAsync();
    }

    public async Task AddRangeAsync(IEnumerable<DriverInduction> records)
    {
        _db.DriverInductions.AddRange(records);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid ownerUserId, Guid driverId, DriverInduction record)
    {
        // Ensure record belongs to the correct owner + driver
        var existing = await _db.DriverInductions.FirstOrDefaultAsync(x =>
            x.Id == record.Id &&
            x.OwnerUserId == ownerUserId &&
            x.DriverId == driverId);

        if (existing == null)
            throw new InvalidOperationException("Induction not found for this driver.");

        // Apply updates via domain methods
        existing.SetStatus(record.Status);
        existing.SetCompletedOnUtc(record.CompletedOnUtc);
        existing.SetExpiresOnUtc(record.ExpiresOnUtc);
        existing.SetNotes(record.Notes);

        // Evidence too if you want
        // existing.SetEvidence(record.EvidenceFilePath);

        await _db.SaveChangesAsync();
    }

    #endregion

    #region Existence Checks

    public async Task<bool> ExistsAsync(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId)
    {
        return await _db.DriverInductions.AnyAsync(x =>
            x.OwnerUserId == ownerUserId &&
            x.DriverId == driverId &&
            x.WorkSiteId == workSiteId &&
            x.RequirementId == requirementId);
    }

    #endregion

    #region Queries

    public async Task<IReadOnlyList<DriverInductionListItemDto>> GetListItemsByDriverAsync(Guid ownerUserId, Guid driverId)
    {
        // Join inductions with worksites and requirements to build a UI-ready DTO list
        var rows = await _db.DriverInductions
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId && x.DriverId == driverId)
            .Join(_db.WorkSites.AsNoTracking(),
                di => di.WorkSiteId,
                ws => ws.Id,
                (di, ws) => new { di, ws })
            .Join(_db.InductionRequirements.AsNoTracking(),
                x => x.di.RequirementId,
                r => r.Id,
                (x, r) => new DriverInductionListItemDto
                {
                    DriverInductionId = x.di.Id,
                    WorkSiteId = x.ws.Id,
                    WorkSiteName = x.ws.Name,

                    RequirementId = r.Id,
                    RequirementTitle = r.Title,

                    // ✅ NEW: include company name (optional)
                    CompanyName = r.CompanyName,

                    ValidForDays = r.ValidForDays,
                    PpeRequired = r.PpeRequired,

                    Status = x.di.Status,

                    // Include issue date so UI can display/edit later
                    IssueDateUtc = x.di.IssueDateUtc,

                    CompletedOnUtc = x.di.CompletedOnUtc,
                    ExpiresOnUtc = x.di.ExpiresOnUtc
                })
            .OrderBy(x => x.WorkSiteName)
            .ThenBy(x => x.RequirementTitle)
            .ToListAsync();

        // Compute "days left" safely in C#
        var utcNow = DateTime.UtcNow;
        foreach (var dto in rows)
        {
            if (dto.ExpiresOnUtc.HasValue)
                dto.DaysLeft = (int)Math.Floor((dto.ExpiresOnUtc.Value - utcNow).TotalDays);
            else
                dto.DaysLeft = null;
        }

        return rows;
    }

    public async Task<DriverInduction?> GetAsync(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId)
    {
        return await _db.DriverInductions
            .FirstOrDefaultAsync(x =>
                x.OwnerUserId == ownerUserId &&
                x.DriverId == driverId &&
                x.WorkSiteId == workSiteId &&
                x.RequirementId == requirementId);
    }

    public async Task<Dictionary<Guid, int>> CountExpiringSoonByDriverAsync(Guid ownerUserId, int withinDays)
    {
        if (ownerUserId == Guid.Empty)
            return new Dictionary<Guid, int>();

        var utcNow = DateTime.UtcNow;
        var cutoff = utcNow.AddDays(withinDays);

        return await _db.DriverInductions
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId
                     && x.ExpiresOnUtc.HasValue
                     && x.ExpiresOnUtc.Value > utcNow
                     && x.ExpiresOnUtc.Value <= cutoff)
            .GroupBy(x => x.DriverId)
            .Select(g => new { DriverId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DriverId, x => x.Count);
    }

    // NOTE: This method is not present in the interface you posted earlier.
    // Add it to IDriverInductionRepository or remove it from this class.
    public async Task<Dictionary<Guid, int>> CountExpiredByDriverAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return new Dictionary<Guid, int>();

        var utcNow = DateTime.UtcNow;

        return await _db.DriverInductions
            .AsNoTracking()
            .Where(x => x.OwnerUserId == ownerUserId
                     && x.ExpiresOnUtc.HasValue
                     && x.ExpiresOnUtc.Value <= utcNow)
            .GroupBy(x => x.DriverId)
            .Select(g => new { DriverId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.DriverId, x => x.Count);
    }

    #endregion
}