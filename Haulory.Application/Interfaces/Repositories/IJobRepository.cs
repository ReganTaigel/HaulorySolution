using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

#region Interface: Job Repository

public interface IJobRepository
{
    #region Create

    Task AddAsync(Job job);

    #endregion

    #region Queries

    // Active = NOT delivered yet (your "Active Deliveries" list)
    Task<IReadOnlyList<Job>> GetActiveByOwnerAsync(Guid ownerUserId);

    // Optional (but useful): active jobs assigned to a driver
    Task<IReadOnlyList<Job>> GetActiveByDriverAsync(Guid ownerUserId, Guid driverId);

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

    Task DeleteAsync(Guid id);

    #endregion
}

#endregion