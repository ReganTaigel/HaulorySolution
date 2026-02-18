using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

#region Interface: Job Repository

public interface IJobRepository
{
    #region Create

    // Adds a new job to storage
    Task AddAsync(Job job);

    #endregion

    #region Queries

    // Retrieves all jobs (typically scoped by owner in implementation)
    Task<IReadOnlyList<Job>> GetAllAsync();

    // Retrieves a specific job by Id
    Task<Job?> GetByIdAsync(Guid id);

    #endregion

    #region Ordering

    // Updates multiple jobs (used when reordering drag/drop lists)
    Task UpdateAllAsync(IReadOnlyList<Job> jobs);

    // Returns the next available sort order value
    Task<int> GetNextSortOrderAsync();

    #endregion

    #region Updates

    // Updates a job (e.g., status, signature, delivery completion)
    Task UpdateAsync(Job job);

    #endregion

    #region Lifecycle

    // Deletes a job by Id
    Task DeleteAsync(Guid id);

    #endregion
}

#endregion
