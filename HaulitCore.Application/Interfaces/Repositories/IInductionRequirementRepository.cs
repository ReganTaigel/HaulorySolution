using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Interfaces.Repositories;

#region Interface: Induction Requirement Repository

public interface IInductionRequirementRepository
{
    #region Create

    // Adds a new induction requirement
    Task AddAsync(InductionRequirement req);

    #endregion

    #region Queries

    // Retrieves all active requirements for an owner
    // Used when seeding driver compliance
    Task<IReadOnlyList<InductionRequirement>>
        GetActiveByOwnerAsync(Guid ownerUserId);

    // Retrieves active requirements for a specific worksite
    // Used when assigning site-specific inductions
    Task<IReadOnlyList<InductionRequirement>>
        GetActiveBySiteAsync(Guid ownerUserId, Guid workSiteId);

    #endregion
}

#endregion
