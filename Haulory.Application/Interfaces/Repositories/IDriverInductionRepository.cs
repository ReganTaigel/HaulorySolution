using Haulory.Application.Features.Incductions;
using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

#region Interface: Driver Induction Repository

public interface IDriverInductionRepository
{
    #region Create

    // Adds a single driver induction record
    Task AddAsync(DriverInduction record);

    // Adds multiple driver induction records in batch
    Task AddRangeAsync(IEnumerable<DriverInduction> records);

    #endregion

    #region Existence Checks

    // Checks if a specific induction already exists
    // Prevents duplicate records per driver/worksite/requirement
    Task<bool> ExistsAsync(
        Guid ownerUserId,
        Guid driverId,
        Guid workSiteId,
        Guid requirementId);

    #endregion

    #region Queries

    // Returns UI-ready list items for a specific driver
    // Uses projection (DTO) instead of returning full domain entities
    Task<IReadOnlyList<DriverInductionListItemDto>>
        GetListItemsByDriverAsync(Guid ownerUserId, Guid driverId);

    // Retrieves a specific induction record
    Task<DriverInduction?> GetAsync(
        Guid ownerUserId,
        Guid driverId,
        Guid workSiteId,
        Guid requirementId);

    // Returns a count of expiring inductions grouped by driver
    // Key = DriverId, Value = Count expiring within given days
    Task<Dictionary<Guid, int>>
        CountExpiringSoonByDriverAsync(Guid ownerUserId, int withinDays);

    #endregion

    #region Update

    // Updates an existing induction record
    Task UpdateAsync(Guid ownerUserId, Guid driverId, DriverInduction record);

    #endregion

    Task<Dictionary<Guid, int>> CountExpiredByDriverAsync(Guid ownerUserId);

}

#endregion
