using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users;

public class GetSubUsersHandler
{
    private readonly IUserAccountRepository _repository;

    public GetSubUsersHandler(IUserAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<IReadOnlyList<UserAccount>> HandleAsync(Guid ownerMainUserId)
    {
        if (ownerMainUserId == Guid.Empty)
            return Array.Empty<UserAccount>();

        return await _repository.GetSubUsersAsync(ownerMainUserId);
    }
}