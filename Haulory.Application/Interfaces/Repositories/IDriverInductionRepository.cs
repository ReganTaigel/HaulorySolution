using Haulory.Application.Features.Incductions;
using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IDriverInductionRepository
{
    Task AddAsync(DriverInduction record);
    Task AddRangeAsync(IEnumerable<DriverInduction> records);
    Task<bool> ExistsAsync(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId);
    Task<IReadOnlyList<DriverInductionListItemDto>> GetListItemsByDriverAsync(Guid ownerUserId, Guid driverId);
    Task<DriverInduction?> GetAsync(Guid ownerUserId, Guid driverId, Guid workSiteId, Guid requirementId);
    Task UpdateAsync(Guid ownerUserId, Guid driverId, DriverInduction record);
    Task<Dictionary<Guid, int>> CountExpiringSoonByDriverAsync(Guid ownerUserId, int withinDays);
}
