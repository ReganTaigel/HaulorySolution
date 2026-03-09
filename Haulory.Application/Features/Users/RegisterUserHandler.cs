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
        var email = command.Email?.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email))
            return new(false, "Email is required.");

        if (string.IsNullOrWhiteSpace(command.FirstName))
            return new(false, "First name is required.");

        if (string.IsNullOrWhiteSpace(command.LastName))
            return new(false, "Last name is required.");

        if (string.IsNullOrWhiteSpace(command.BusinessName))
            return new(false, "Business name is required.");

        if (!PasswordPolicy.IsValid(command.Password, out var error))
            return new(false, error ?? "Password does not meet requirements.");

        var existing = await _userRepository.GetByEmailAsync(email);
        if (existing != null)
            return new(false, "A user with this email already exists.");

        var hash = PasswordHasher.Hash(command.Password);

        var user = new UserAccount(
            firstName: command.FirstName.Trim(),
            lastName: command.LastName.Trim(),
            email: email,
            passwordHash: hash
        );

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

        await _userRepository.AddAsync(user);

        await _createDriverFromUserHandler.HandleAsync(
            new CreateDriverFromUserCommand(
                UserId: user.Id,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,

                PhoneNumber: null,
                DateOfBirthUtc: null,

                LicenceNumber: null,
                LicenceVersion: null,
                LicenceClassOrEndorsements: null,
                LicenceIssuedOnUtc: null,
                LicenceExpiresOnUtc: null,
                LicenceConditionsNotes: null,

                Line1: null,
                Line2: null,
                Suburb: null,
                City: null,
                Region: null,
                Postcode: null,
                Country: null
            ));

        return new(true, null);
    }
}