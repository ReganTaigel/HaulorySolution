using Haulory.Application.Interfaces.Repositories;
using Haulory.Core.Security;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users;

public class CreateSubUserHandler
{
    private readonly IUserAccountRepository _repo;

    public CreateSubUserHandler(IUserAccountRepository repo)
    {
        _repo = repo;
    }

    public async Task<UserAccount?> HandleAsync(CreateSubUserCommand command)
    {
        if (command.OwnerMainUserId == Guid.Empty)
            return null;

        var email = command.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return null;

        if (string.IsNullOrWhiteSpace(command.Password))
            return null;

        // prevent duplicates
        var existing = await _repo.GetByEmailAsync(email);
        if (existing != null)
            return null;

        var hash = PasswordHasher.Hash(command.Password);

        var sub = UserAccount.CreateSubUser(
            parentMainUserId: command.OwnerMainUserId,
            firstName: command.FirstName,
            lastName: command.LastName,
            email: email,
            passwordHash: hash
        );

        await _repo.AddAsync(sub);
        return sub;
    }
}