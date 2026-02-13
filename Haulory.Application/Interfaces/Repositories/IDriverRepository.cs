using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories
{
    public interface IDriverRepository
    {
        Task<List<Driver>> GetAllAsync();

        // Main user's driver profile lookup (driver.UserId == main user's UserId)
        Task<Driver?> GetByUserIdAsync(Guid userId);

        // Only drivers owned by the given main user
        Task<List<Driver>> GetAllByOwnerUserIdAsync(Guid ownerUserId);

        Task SaveAsync(Driver driver);
    }
}
