using HaulitCore.Application.Features.Drivers;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Security;
using HaulitCore.Core.Security;
using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Features.Users;

// Handles the user registration use case.
// Creates a new main user account and automatically provisions a linked driver profile.
public class RegisterUserHandler
{
    // Repository for user account persistence.
    private readonly IUserAccountRepository _userRepository;

    // Handler used to create a driver profile for the newly registered user.
    private readonly CreateDriverFromUserHandler _createDriverFromUserHandler;

    // Result type representing success/failure of registration.
    public record RegisterUserResult(bool Success, string? Error);

    // Constructor injection of dependencies.
    public RegisterUserHandler(
        IUserAccountRepository userRepository,
        CreateDriverFromUserHandler createDriverFromUserHandler)
    {
        _userRepository = userRepository;
        _createDriverFromUserHandler = createDriverFromUserHandler;
    }

    // Registers a new user and provisions associated domain entities.
    public async Task<RegisterUserResult> HandleAsync(RegisterUserCommand command)
    {
        // Normalise and validate email.
        var email = command.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return new(false, "Email is required.");

        // Validate required identity fields.
        if (string.IsNullOrWhiteSpace(command.FirstName))
            return new(false, "First name is required.");

        if (string.IsNullOrWhiteSpace(command.LastName))
            return new(false, "Last name is required.");

        // Validate required business identity.
        if (string.IsNullOrWhiteSpace(command.BusinessName))
            return new(false, "Business name is required.");

        // Enforce password policy.
        if (!PasswordPolicy.IsValid(command.Password, out var error))
            return new(false, error ?? "Password does not meet requirements.");

        // Ensure email uniqueness.
        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            return new(false, "A user with this email already exists.");

        // Hash password before storing.
        var hash = PasswordHasher.Hash(command.Password);

        // Create the main user account.
        var user = new UserAccount(
            firstName: command.FirstName.Trim(),
            lastName: command.LastName.Trim(),
            email: email,
            passwordHash: hash
        );

        // Apply business identity details.
        user.UpdateBusinessIdentity(
            businessName: command.BusinessName,
            businessEmail: command.BusinessEmail,
            gstNumber: command.SupplierGstNumber,
            nzbn: command.SupplierNzbn,
            bankAccountNumber: command.BankAccountNumber
        );

        // Apply business contact details.
        user.UpdateBusinessContact(command.BusinessPhone);

        // Apply business address details.
        user.UpdateBusinessAddress(
            line1: command.BusinessAddress1,
            suburb: command.BusinessSuburb,
            city: command.BusinessCity,
            region: command.BusinessRegion,
            postcode: command.BusinessPostcode,
            country: command.BusinessCountry
        );

        // Persist the new user account.
        await _userRepository.AddAsync(user);

        // Automatically create a linked driver profile for the new user.
        await _createDriverFromUserHandler.HandleAsync(
            new CreateDriverFromUserCommand(
                UserId: user.Id,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,

                // No additional profile data provided at registration.
                PhoneNumber: null,
                DateOfBirthUtc: null,

                LicenceNumber: null,
                LicenceVersion: null,
                LicenceClassOrEndorsements: null,
                LicenceIssuedOnUtc: null,
                LicenceExpiresOnUtc: null,
                LicenceConditionsNotes: null,

                Line1: null,
                Suburb: null,
                City: null,
                Region: null,
                Postcode: null,
                Country: null
            ));

        // Return success result.
        return new(true, null);
    }
}