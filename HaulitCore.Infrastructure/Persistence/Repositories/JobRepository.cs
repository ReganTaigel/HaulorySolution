using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HaulitCore.Infrastructure.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly HaulitCoreDbContext _db;

    public JobRepository(HaulitCoreDbContext db)
    {
        _db = db;
    }

    #region Commands

    public async Task AddAsync(Job job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        var tracked = _db.Jobs.Local.FirstOrDefault(j => j.Id == job.Id);
        if (tracked != null)
            return;

        var exists = await _db.Jobs.AnyAsync(j => j.Id == job.Id);
        if (exists)
            return;

        _db.Jobs.Add(job);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAsync(Job job)
    {
        if (job == null)
            throw new ArgumentNullException(nameof(job));

        if (job.Id == Guid.Empty)
            throw new ArgumentException("Job.Id required.", nameof(job));

        try
        {
            // Force EF to recalculate collection/orphan changes first.
            _db.ChangeTracker.DetectChanges();
            _db.ChangeTracker.CascadeChanges();

            // The job aggregate now contains only the assignments that should exist.
            var currentAssignmentIds = job.TrailerAssignments
                .Select(x => x.Id)
                .ToHashSet();

            // Any tracked trailer assignment for this job that is no longer in the aggregate
            // must be deleted, not updated.
            foreach (var entry in _db.ChangeTracker.Entries<JobTrailerAssignment>()
                         .Where(e => e.Entity.JobId == job.Id))
            {
                var stillInAggregate = currentAssignmentIds.Contains(entry.Entity.Id);

                if (!stillInAggregate && entry.State != EntityState.Added)
                {
                    entry.State = EntityState.Deleted;
                }
            }

            // Run change detection again after state corrections.
            _db.ChangeTracker.DetectChanges();
            _db.ChangeTracker.CascadeChanges();

            await _db.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine("DbUpdateConcurrencyException while saving job.");

            foreach (var entry in ex.Entries)
            {
                sb.AppendLine($"Entity: {entry.Metadata.ClrType.Name}");
                sb.AppendLine($"State: {entry.State}");

                var keyProps = entry.Properties.Where(p => p.Metadata.IsPrimaryKey());
                foreach (var keyProp in keyProps)
                {
                    sb.AppendLine($"Key {keyProp.Metadata.Name}: {keyProp.CurrentValue}");
                }

                foreach (var prop in entry.Properties)
                {
                    sb.AppendLine(
                        $"Property {prop.Metadata.Name} | Current={prop.CurrentValue} | Original={prop.OriginalValue}");
                }

                sb.AppendLine("----");
            }

            throw new InvalidOperationException(sb.ToString(), ex);
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        var job = await _db.Jobs.FindAsync(id);
        if (job == null)
            return;

        _db.Jobs.Remove(job);
        await _db.SaveChangesAsync();
    }

    public async Task UpdateAllAsync(Guid ownerUserId, IReadOnlyList<Job> jobs)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("ownerUserId required.");

        if (jobs == null)
            throw new ArgumentNullException(nameof(jobs));

        if (jobs.Any(j => j.OwnerUserId != ownerUserId))
            throw new InvalidOperationException("UpdateAllAsync contains jobs from a different owner.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var existing = await _db.Jobs
            .Where(j => j.OwnerUserId == ownerUserId)
            .ToListAsync();

        var incomingById = jobs.ToDictionary(j => j.Id, j => j.SortOrder);

        foreach (var e in existing)
        {
            if (incomingById.TryGetValue(e.Id, out var newOrder))
                e.SetSortOrder(newOrder);
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }

    #endregion

    #region Queries

    public async Task<IReadOnlyList<Job>> GetActiveByOwnerAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return Array.Empty<Job>();

        return await _db.Jobs
            .AsNoTracking()
            .Include(j => j.TrailerAssignments)
            .Where(j => j.OwnerUserId == ownerUserId)
            .Where(j => j.Status == JobStatus.Active)
            .OrderBy(j => j.SortOrder)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Job>> GetActiveAssignedToUserAsync(Guid ownerUserId, Guid assignedToUserId)
    {
        if (ownerUserId == Guid.Empty || assignedToUserId == Guid.Empty)
            return Array.Empty<Job>();

        return await _db.Jobs
            .AsNoTracking()
            .Include(j => j.TrailerAssignments)
            .Where(j => j.OwnerUserId == ownerUserId)
            .Where(j => j.AssignedToUserId == assignedToUserId)
            .Where(j => j.Status == JobStatus.Active)
            .OrderBy(j => j.SortOrder)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Job>> GetNeedsReviewAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            return Array.Empty<Job>();

        return await _db.Jobs
            .AsNoTracking()
            .Include(j => j.TrailerAssignments)
            .Where(j => j.OwnerUserId == ownerUserId)
            .Where(j => j.Status == JobStatus.DeliveredPendingReview)
            .OrderByDescending(j => j.DeliveredAtUtc)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Job>> GetActiveByDriverAsync(Guid ownerUserId, Guid driverId)
    {
        if (ownerUserId == Guid.Empty || driverId == Guid.Empty)
            return Array.Empty<Job>();

        return await _db.Jobs
            .AsNoTracking()
            .Include(j => j.TrailerAssignments)
            .Where(j => j.OwnerUserId == ownerUserId)
            .Where(j => j.DriverId == driverId)
            .Where(j => j.Status == JobStatus.Active)
            .OrderBy(j => j.SortOrder)
            .ToListAsync();
    }

    public async Task<Job?> GetByIdAsync(Guid id)
    {
        return await _db.Jobs
            .AsNoTracking()
            .Include(j => j.TrailerAssignments)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<Job?> GetByIdForUpdateAsync(Guid id)
    {
        return await _db.Jobs
            .FirstOrDefaultAsync(j => j.Id == id);
    }
    public async Task SyncTrailerAssignmentsAsync(Guid jobId, IReadOnlyList<Guid> trailerIds)
    {
        if (jobId == Guid.Empty)
            throw new ArgumentException("JobId required.", nameof(jobId));

        var distinct = (trailerIds ?? Array.Empty<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (distinct.Count > 2)
            throw new InvalidOperationException("A maximum of 2 trailers can be assigned to a job.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var existing = await _db.JobTrailerAssignments
            .Where(x => x.JobId == jobId)
            .ToListAsync();

        if (existing.Count > 0)
        {
            _db.JobTrailerAssignments.RemoveRange(existing);
            await _db.SaveChangesAsync();
        }

        var position = 1;
        foreach (var trailerId in distinct)
        {
            _db.JobTrailerAssignments.Add(
                new JobTrailerAssignment(jobId, trailerId, position++));
        }

        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }
    public async Task<int> GetNextSortOrderAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("ownerUserId required.");

        var max = await _db.Jobs
            .Where(j => j.OwnerUserId == ownerUserId)
            .MaxAsync(j => (int?)j.SortOrder);

        return (max ?? 0) + 1;
    }

    #endregion

    #region Invoice

    public async Task<bool> InvoiceNumberExistsAsync(Guid ownerUserId, string invoiceNumber)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("ownerUserId required.");

        var token = invoiceNumber.Trim();

        return await _db.Jobs.AnyAsync(j =>
            j.OwnerUserId == ownerUserId &&
            j.InvoiceNumber == token);
    }

    public async Task<string?> GetLatestInvoiceNumberAsync(Guid ownerUserId)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("ownerUserId required.");

        return await _db.Jobs
            .AsNoTracking()
            .Where(j => j.OwnerUserId == ownerUserId && !string.IsNullOrWhiteSpace(j.InvoiceNumber))
            .OrderByDescending(j => j.InvoiceNumber)
            .Select(j => j.InvoiceNumber)
            .FirstOrDefaultAsync();
    }

    #endregion
}