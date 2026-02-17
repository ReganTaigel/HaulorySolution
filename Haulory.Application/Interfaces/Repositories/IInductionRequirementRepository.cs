using Haulory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;
public interface IInductionRequirementRepository
{
    Task AddAsync(InductionRequirement req);
    Task<IReadOnlyList<InductionRequirement>> GetActiveByOwnerAsync(Guid ownerUserId);
    Task<IReadOnlyList<InductionRequirement>> GetActiveBySiteAsync(Guid ownerUserId, Guid workSiteId);
}
