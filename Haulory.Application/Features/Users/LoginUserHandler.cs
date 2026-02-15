using Haulory.Application.Interfaces.Repositories;
using Haulory.Core.Security;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users;

public class LoginUserHandler
{
    private readonly IUserAccountRepository _repository;

    public LoginUserHandler(IUserAccountRepository repository)
    {
        _repository = repository;
    }

    public async Task<UserAccount?> HandleAsync(LoginUserCommand command)
    {
        var email = command.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(email))
            return null;

        var user = await _repository.GetByEmailAsync(email);

        if (user == null)
            return null;

        if (!PasswordHasher.Verify(command.Password, user.PasswordHash))
            return null;

        return user; // SUCCESS
    }
}
