using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

#region Interface: Driver Repository

public interface IDriverRepository
{
    #region Queries

    // Retrieves all drivers (admin-level usage)
    Task<List<Driver>> GetAllAsync();

    // Retrieves driver profile by linked UserId
    // Used when driver has a login account
    Task<Driver?> GetByUserIdAsync(Guid userId);

    // Retrieves all drivers owned by a specific main account
    // Ensures tenant isolation
    Task<List<Driver>> GetAllByOwnerUserIdAsync(Guid ownerUserId);

    // Retrieves a specific driver by Id
    Task<Driver> GetByIdAsync(Guid id);

    #endregion

    #region Persistence

    // Saves a driver (create or update depending on implementation)
    Task SaveAsync(Driver driver);

    #endregion
}

#endregion
