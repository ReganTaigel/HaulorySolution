using Haulory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

public interface IWorkSiteRepository
{
    Task AddAsync(WorkSite site);
    Task<IReadOnlyList<WorkSite>> GetAllByOwnerAsync(Guid ownerUserId);
    Task<WorkSite?> GetByIdAsync(Guid ownerUserId, Guid workSiteId);
}