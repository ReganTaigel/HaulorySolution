using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IDriverRepository
{
    Task<List<Driver>> GetAllAsync();

    Task<Driver?> GetByUserIdAsync(Guid userId);

    Task<List<Driver>> GetAllByOwnerUserIdAsync(Guid ownerUserId);

    Task<Driver> GetByIdAsync(Guid id);

    // NEW
    Task<int> CountMainDriversAsync(Guid ownerUserId);

    // NEW
    Task<int> CountSubDriversAsync(Guid ownerUserId);

    Task SaveAsync(Driver driver);
}