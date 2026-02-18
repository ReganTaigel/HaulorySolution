using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Security;
using Haulory.Core.Security;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users;

public class RegisterUserHandler
{
    #region Dependencies

    private readonly IUserAccountRepository _userRepository;
    private readonly CreateDriverFromUserHandler _createDriverFromUserHandler;

    #endregion

    #region Result

    // Simple result contract for UI/API: avoids throwing for validation failures
    public record RegisterUserResult(bool Success, string? Error);

    #endregion

    #region Constructor

    public RegisterUserHandler(
        IUserAccountRepository userRepository,
        CreateDriverFromUserHandler createDriverFromUserHandler)
    {
        _userRepository = userRepository;
        _createDriverFromUserHandler = createDriverFromUserHandler;
    }

    #endregion

    #region Public API

    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command)
    {
        #region Validation

        // Normalize email early for consistent uniqueness checks
        var email = command.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return new(false, "Email is required.");

        if (string.IsNullOrWhiteSpace(command.FirstName))
            return new(false, "First name is required.");

        if (string.IsNullOrWhiteSpace(command.LastName))
            return new(false, "Last name is required.");

        // Enforce password policy (centralized rules)
        if (!PasswordPolicy.IsValid(command.Password, out var error))
            return new(false, error ?? "Password does not meet requirements.");

        #endregion

        #region Uniqueness Check

        // Prevent duplicate accounts
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            return new(false, "A user with this email already exists.");

        #endregion

        #region Create User

        // Hash password before storing
        var hash = PasswordHasher.Hash(command.Password);

        var user = new UserAccount(
            command.FirstName.Trim().ToUpperInvariant(),
            command.LastName.Trim().ToUpperInvariant(),
            email,
            hash
        );

        await _userRepository.AddAsync(user);

        #endregion

        #region Create Driver Profile (Idempotent)

        // Create Driver profile for Main user (handler is idempotent: returns existing if already present)
        await _createDriverFromUserHandler.HandleAsync(
            new CreateDriverFromUserCommand(
                user.Id,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty
            )
        );

        #endregion

        return new(true, null);
    }

    #endregion
}
