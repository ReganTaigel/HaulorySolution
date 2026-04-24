using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Interfaces.Repositories;

#region Interface: User Account Repository

public interface IUserAccountRepository
{
    #region Existence

    // Returns true if any user accounts exist (useful for initial setup / seeding)
    Task<bool> AnyAsync();

    #endregion

    #region Create / Update

    // Adds a new user account
    Task AddAsync(UserAccount user);

    // Updates an existing user account
    Task UpdateAsync(UserAccount user);

    Task SaveChangesAsync();
    #endregion

    #region Queries

    // Retrieves a user account by Id
    Task<UserAccount?> GetByIdAsync(Guid id);

    // Retrieves a user account by email (email should be normalized by caller)
    Task<UserAccount?> GetByEmailAsync(string? email);

    // Retrieves sub-users linked to a main account
    Task<IReadOnlyList<UserAccount>> GetSubUsersAsync(Guid mainUserId);

    #endregion
}

#endregion
