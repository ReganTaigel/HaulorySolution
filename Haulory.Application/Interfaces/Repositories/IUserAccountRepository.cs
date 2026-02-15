using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Repositories;

public interface IUserAccountRepository
{
    Task<bool> AnyAsync();
    Task AddAsync(UserAccount user);
    Task UpdateAsync(UserAccount user);
    Task<UserAccount?> GetByIdAsync(Guid id);
    Task<UserAccount?> GetByEmailAsync(string? email);
    Task<IReadOnlyList<UserAccount>> GetSubUsersAsync(Guid mainUserId);
}
