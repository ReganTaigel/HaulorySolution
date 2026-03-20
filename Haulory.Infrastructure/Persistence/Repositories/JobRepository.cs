using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Infrastructure.Persistence.Repositories;

public class JobRepository : IJobRepository
{
    private readonly HauloryDbContext _db;

    public JobRepository(HauloryDbContext db)
    {
        _db = db;
    }

    #region Commands

    public async Task AddAsync(Job job)
    {
        if (job == null) throw new ArgumentNullException(nameof(job));

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
        if (job == null) throw new ArgumentNullException(nameof(job));
        if (job.Id == Guid.Empty) throw new ArgumentException("Job.Id required.", nameof(job));

        var target = _db.Jobs.Local.FirstOrDefault(j => j.Id == job.Id)
                  ?? await _db.Jobs.FirstOrDefaultAsync(j => j.Id == job.Id);

        if (target == null)
            throw new KeyNotFoundException($"Job not found: {job.Id}");

        if (target.OwnerUserId != job.OwnerUserId)
            throw new InvalidOperationException("OwnerUserId mismatch.");

        // Assignment / allocation
        target.AssignToSubUser(job.AssignedToUserId);
        target.AssignDriver(job.DriverId);
        target.AssignVehicle(job.VehicleAssetId);

        // Ordering
        target.SetSortOrder(job.SortOrder);

        // ✅ Delivery completion (run ONCE only)
        // If already delivered, do not overwrite DeliveredAtUtc/status.
        var incomingHasDelivery = !string.IsNullOrWhiteSpace(job.DeliverySignatureJson)
                                  && !string.IsNullOrWhiteSpace(job.ReceiverName);

        if (target.DeliveredAtUtc == null && incomingHasDelivery)
        {
            if (!job.DeliveredByUserId.HasValue || job.DeliveredByUserId.Value == Guid.Empty)
                throw new InvalidOperationException("DeliveredByUserId is required when completing delivery.");

            target.CompleteDelivery(
                job.DeliveredByUserId.Value,
                job.ReceiverName!.Trim(),
                job.DeliverySignatureJson!,
                job.WaitTimeMinutes,
                job.DamageNotes
            );
        }

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
            .Where(j => j.Status == JobStatus.Active) // ✅ use Status (not DeliveredAtUtc)
            .OrderBy(j => j.SortOrder)
            .ToListAsync();
    }

    // ✅ Sub user: only their assigned active jobs
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

    // ✅ Main user review inbox: delivered with exceptions
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
            .Include(j => j.TrailerAssignments)
            .FirstOrDefaultAsync(j => j.Id == id);
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