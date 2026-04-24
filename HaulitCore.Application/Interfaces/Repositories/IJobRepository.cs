using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Interfaces.Repositories;

public interface IJobRepository
{
    Task AddAsync(Job job);

    Task<IReadOnlyList<Job>> GetActiveByOwnerAsync(Guid ownerUserId);
    Task<IReadOnlyList<Job>> GetActiveByDriverAsync(Guid ownerUserId, Guid driverId);
    Task<IReadOnlyList<Job>> GetActiveAssignedToUserAsync(Guid ownerUserId, Guid assignedToUserId);
    Task<IReadOnlyList<Job>> GetNeedsReviewAsync(Guid ownerUserId);
    Task SyncTrailerAssignmentsAsync(Guid jobId, IReadOnlyList<Guid> trailerIds);
    Task<Job?> GetByIdAsync(Guid id);

    // Must include TrailerAssignments for edit workflows
    Task<Job?> GetByIdForUpdateAsync(Guid id);

    Task<int> GetNextSortOrderAsync(Guid ownerUserId);
    Task<bool> InvoiceNumberExistsAsync(Guid ownerUserId, string invoiceNumber);
    Task<string?> GetLatestInvoiceNumberAsync(Guid ownerUserId);

    Task UpdateAsync(Job job);
    Task DeleteAsync(Guid id);
    Task UpdateAllAsync(Guid ownerUserId, IReadOnlyList<Job> jobs);
}