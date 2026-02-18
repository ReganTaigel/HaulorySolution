using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

#region Interface: Work Site Repository

public interface IWorkSiteRepository
{
    #region Create

    // Adds a new worksite for an owner
    Task AddAsync(WorkSite site);

    #endregion

    #region Queries

    // Retrieves all worksites belonging to a specific owner
    Task<IReadOnlyList<WorkSite>> GetAllByOwnerAsync(Guid ownerUserId);

    // Retrieves a specific worksite by Id, scoped to owner
    Task<WorkSite?> GetByIdAsync(Guid ownerUserId, Guid workSiteId);

    #endregion
}

#endregion
