using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Core.Security;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users;

public class LoginUserHandler
{
    #region Dependencies

    private readonly IUserAccountRepository _repository;
    private readonly ISessionService _session;

    #endregion

    #region Constructor

    public LoginUserHandler(
        IUserAccountRepository repository,
        ISessionService session)
    {
        _repository = repository;
        _session = session;
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

        // Tenant resolution:
        // - main user: owner = user.Id
        // - sub user:  owner = ParentMainUserId
        var ownerId = user.ParentMainUserId ?? user.Id;

        // Persist session (account + tenant)
        await _session.SetAccountAsync(user.Id, ownerId);

        // SUCCESS - return authenticated user
        return user;
    }

    #endregion
}