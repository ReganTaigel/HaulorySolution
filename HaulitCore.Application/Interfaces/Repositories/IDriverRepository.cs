using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Interfaces.Repositories;

public interface IDriverRepository
{
    // Avoid this in normal UI; keep for admin/debug only
    Task<List<Driver>> GetAllAsync();

    Task<Driver?> GetByUserIdAsync(Guid userId);

    // ✅ tenant-safe list
    Task<List<Driver>> GetAllByOwnerUserIdAsync(Guid ownerUserId);

    // ✅ make nullable (don’t throw)
    Task<Driver?> GetByIdAsync(Guid id);

    // ✅ best practice: tenant-safe single fetch
    Task<Driver?> GetByIdForOwnerAsync(Guid ownerUserId, Guid driverId);

    Task<int> CountMainDriversAsync(Guid ownerUserId);
    Task<int> CountSubDriversAsync(Guid ownerUserId);

    Task SaveAsync(Driver driver);


}