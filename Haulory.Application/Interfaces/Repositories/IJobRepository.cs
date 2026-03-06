using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

#region Interface: Job Repository

public interface IJobRepository
{
    #region Create

    Task AddAsync(Job job);

    #endregion

    #region Queries

    // Active = jobs the driver still needs to act on (Status == Active)
    Task<IReadOnlyList<Job>> GetActiveByOwnerAsync(Guid ownerUserId);

    // Active jobs assigned to a driver (optional)
    Task<IReadOnlyList<Job>> GetActiveByDriverAsync(Guid ownerUserId, Guid driverId);

    // ✅ Sub-user: only their assigned active jobs
    Task<IReadOnlyList<Job>> GetActiveAssignedToUserAsync(Guid ownerUserId, Guid assignedToUserId);

    // ✅ Main-user review inbox: delivered with exceptions
    Task<IReadOnlyList<Job>> GetNeedsReviewAsync(Guid ownerUserId);

    Task<Job?> GetByIdAsync(Guid id);

    // Tracked entity for mutation + save
    Task<Job?> GetByIdForUpdateAsync(Guid id);

    #endregion

    #region Ordering

    // Updates sort orders ONLY for this owner (no deletes)
    Task UpdateAllAsync(Guid ownerUserId, IReadOnlyList<Job> jobs);

    Task<int> GetNextSortOrderAsync(Guid ownerUserId);

    #endregion

    #region Updates

    Task UpdateAsync(Job job);

    #endregion

    #region Invoice

    Task<bool> InvoiceNumberExistsAsync(Guid ownerUserId, string invoiceNumber);

    #endregion

    #region Lifecycle

    // Keep for admin/cleanup; do NOT use for normal delivery completion anymore
    Task DeleteAsync(Guid id);

    #endregion
}

#endregion