using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users;

// Handles the application use case for retrieving all sub-users
// belonging to a specific main/owner account (tenant).
public class GetSubUsersHandler
{
    // Repository for user account queries.
    private readonly IUserAccountRepository _repository;

    // Constructor injection of dependencies.
    public GetSubUsersHandler(IUserAccountRepository repository)
    {
        _repository = repository;
    }

    // Retrieves all sub-user accounts for the specified owner.
    public async Task<IReadOnlyList<UserAccount>> HandleAsync(Guid ownerMainUserId)
    {
        // Return empty result if owner ID is invalid.
        if (ownerMainUserId == Guid.Empty)
            return Array.Empty<UserAccount>();

        // Query repository for sub-users under this tenant.
        return await _repository.GetSubUsersAsync(ownerMainUserId);
    }
}