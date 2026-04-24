using System;
using HaulitCore.Application.Features.Incductions;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public class DriverInductionRepository : IDriverInductionRepository
{
    private readonly HaulitCoreDbContext _db;

    public DriverInductionRepository(HaulitCoreDbContext db)
    {
        _db = db;
    }

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
        var existing = await _db.DriverInductions.FirstOrDefaultAsync(x =>
            x.Id == record.Id &&
            x.OwnerUserId == ownerUserId &&
            x.DriverId == driverId);

        if (existing == null)
            throw new InvalidOperationException("Induction not found for this driver.");

        existing.SetStatus(record.Status);
        existing.SetCompletedOnUtc(record.CompletedOnUtc);
        existing.SetExpiresOnUtc(record.ExpiresOnUtc);
        existing.SetNotes(record.Notes);

        if (string.IsNullOrWhiteSpace(record.EvidenceFilePath))
        {
            existing.ClearEvidence();
        }
        else
        {
            existing.SetEvidence(
                record.EvidenceFileName,
                record.EvidenceContentType,
                record.EvidenceFilePath);
        }

        await _db.SaveChangesAsync();
    }

    public async Task<bool> ExistsAsync(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId)
    {
        return await _db.DriverInductions.AnyAsync(x =>
            x.OwnerUserId == ownerUserId &&
            x.DriverId == driverId &&
            x.WorkSiteId == workSiteId &&
            x.RequirementId == requirementId);
    }

    public async Task<IReadOnlyList<DriverInductionListItemDto>> GetListItemsByDriverAsync(Guid ownerUserId, Guid driverId)
    {
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
                    CompanyName = r.CompanyName,
                    ValidForDays = r.ValidForDays,
                    PpeRequired = r.PpeRequired,

                    Status = x.di.Status,
                    IssueDateUtc = x.di.IssueDateUtc,
                    CompletedOnUtc = x.di.CompletedOnUtc,
                    ExpiresOnUtc = x.di.ExpiresOnUtc,

                    EvidenceFileName = x.di.EvidenceFileName,
                    EvidenceContentType = x.di.EvidenceContentType,
                    EvidenceFilePath = x.di.EvidenceFilePath,
                    EvidenceUploadedOnUtc = x.di.EvidenceUploadedOnUtc
                })
            .OrderBy(x => x.WorkSiteName)
            .ThenBy(x => x.RequirementTitle)
            .ToListAsync();

        var utcNow = DateTime.UtcNow;

        foreach (var dto in rows)
        {
            dto.DaysLeft = dto.ExpiresOnUtc.HasValue
                ? (int)Math.Floor((dto.ExpiresOnUtc.Value - utcNow).TotalDays)
                : null;
        }

        return rows;
    }

    public async Task<DriverInduction?> GetAsync(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId)
    {
        return await _db.DriverInductions.FirstOrDefaultAsync(x =>
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
}