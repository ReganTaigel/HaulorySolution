using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IJobRepository
{
    #region Jobs
    Task AddAsync(Job job);
    Task<IReadOnlyList<Job>> GetAllAsync();
    #endregion

    #region Ordering
    Task UpdateAllAsync(IReadOnlyList<Job> jobs);
    Task<int> GetNextSortOrderAsync();
    #endregion

    #region Signature / Delivery
    Task UpdateAsync(Job job);
    Task<Job?> GetByIdAsync(Guid id);
    #endregion

    #region Lifecycle
    Task DeleteAsync(Guid id);
    #endregion

}
