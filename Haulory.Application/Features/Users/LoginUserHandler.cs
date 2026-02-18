using Haulory.Application.Interfaces.Repositories;
using Haulory.Core.Security;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users;

public class LoginUserHandler
{
    #region Dependencies

    private readonly IUserAccountRepository _repository;

    #endregion

    #region Constructor

    public LoginUserHandler(IUserAccountRepository repository)
    {
        _repository = repository;
    }

    #endregion

    #region Public API

    public async Task<UserAccount?> HandleAsync(LoginUserCommand command)
    {
        // Normalize email to ensure consistent lookup
        var email = command.Email?.Trim().ToLowerInvariant();

        // Basic validation
        if (string.IsNullOrWhiteSpace(email))
            return null;

        // Fetch user by email
        var user = await _repository.GetByEmailAsync(email);

        if (user == null)
            return null;

        // Verify hashed password
        if (!PasswordHasher.Verify(command.Password, user.PasswordHash))
            return null;

        // SUCCESS - return authenticated user
        return user;
    }

    #endregion
}
