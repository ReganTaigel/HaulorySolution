using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string? email);
        Task AddAsync(User user);
        Task<bool> AnyAsync();

        Task<User?> GetByIdAsync(Guid id);
        Task UpdateAsync(User user);
    }
}
