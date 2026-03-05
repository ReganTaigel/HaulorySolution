using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Security;
using Haulory.Core.Security;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Users;

public class RegisterUserHandler
{
    private readonly IUserAccountRepository _userRepository;
    private readonly CreateDriverFromUserHandler _createDriverFromUserHandler;

    public record RegisterUserResult(bool Success, string? Error);

    public RegisterUserHandler(
        IUserAccountRepository userRepository,
        CreateDriverFromUserHandler createDriverFromUserHandler)
    {
        _userRepository = userRepository;
        _createDriverFromUserHandler = createDriverFromUserHandler;
    }

    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command)
    {
        // Normalize email early
        var email = command.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return new(false, "Email is required.");

        if (string.IsNullOrWhiteSpace(command.FirstName))
            return new(false, "First name is required.");

        if (string.IsNullOrWhiteSpace(command.LastName))
            return new(false, "Last name is required.");

        // Business name required (your PDFs need it)
        if (string.IsNullOrWhiteSpace(command.BusinessName))
            return new(false, "Business name is required.");

        if (!PasswordPolicy.IsValid(command.Password, out var error))
            return new(false, error ?? "Password does not meet requirements.");

        // Unique email
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            return new(false, "A user with this email already exists.");

        // Hash password
        var hash = PasswordHasher.Hash(command.Password);

        // Create base user
        var user = new UserAccount(
            firstName: command.FirstName.Trim(),
            lastName: command.LastName.Trim(),
            email: email,
            passwordHash: hash
        );

        // Apply business profile
        user.UpdateBusinessIdentity(
            businessName: command.BusinessName,
            businessEmail: command.BusinessEmail,
            gstNumber: command.SupplierGstNumber,
            nzbn: command.SupplierNzbn
        );

        user.UpdateBusinessContact(command.BusinessPhone);

        user.UpdateBusinessAddress(
            line1: command.BusinessAddress1,
            line2: command.BusinessAddress2,
            suburb: command.BusinessSuburb,
            city: command.BusinessCity,
            region: command.BusinessRegion,
            postcode: command.BusinessPostcode,
            country: command.BusinessCountry
        );

        // Persist
        await _userRepository.AddAsync(user);

        // Create driver profile (main)
        await _createDriverFromUserHandler.HandleAsync(
            new CreateDriverFromUserCommand(
                user.Id,
                user.FirstName ?? string.Empty,
                user.LastName ?? string.Empty,
                user.Email ?? string.Empty
            )
        );

        return new(true, null);
    }
}