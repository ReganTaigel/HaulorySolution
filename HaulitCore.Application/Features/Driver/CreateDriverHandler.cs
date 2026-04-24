using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Limits;
using HaulitCore.Application.Security;
using HaulitCore.Core.Security;
using HaulitCore.Domain.Entities;

namespace HaulitCore.Application.Features.Drivers;

// Handles the application use case for creating a new driver.
// Supports both standard driver creation and optional linked login account creation.
public class CreateDriverHandler
{
    // Repository for persisting driver entities.
    private readonly IDriverRepository _repository;

    // Repository for user account operations when creating login-enabled drivers.
    private readonly IUserAccountRepository _users;

    // Ensures required compliance/induction records exist after driver creation.
    private readonly IComplianceEnsurer _complianceEnsurer;

    // Constructor injection of dependencies.
    public CreateDriverHandler(
        IDriverRepository repository,
        IUserAccountRepository users,
        IComplianceEnsurer complianceEnsurer)
    {
        _repository = repository;
        _users = users;
        _complianceEnsurer = complianceEnsurer;
    }

    // Creates a new driver based on the supplied command.
    public async Task<Driver?> HandleAsync(CreateDriverCommand command)
    {
        // Reject invalid owner IDs.
        if (command.OwnerUserId == Guid.Empty)
            return null;

        // Enforce plan limit for sub drivers.
        var subCount = await _repository.CountSubDriversAsync(command.OwnerUserId);
        if (subCount >= PlanLimits.MaxSubDrivers)
            throw new InvalidOperationException("Sub driver limit reached (max 4).");

        // Normalise core driver identity values.
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();
        var email = command.Email?.Trim().ToLowerInvariant();

        // Validate required identity fields.
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            !email.Contains('@'))
            return null;

        // Normalise emergency contact details.
        var ecFirst = command.EmergencyFirstName?.Trim();
        var ecLast = command.EmergencyLastName?.Trim();
        var ecRel = command.EmergencyRelationship?.Trim();
        var ecEmail = command.EmergencyEmail?.Trim().ToLowerInvariant();
        var ecPhone = command.EmergencyPhoneNumber?.Trim();

        var ecPhone2 = string.IsNullOrWhiteSpace(command.EmergencySecondaryPhoneNumber)
            ? null
            : command.EmergencySecondaryPhoneNumber.Trim();

        // Validate required emergency contact fields.
        if (string.IsNullOrWhiteSpace(ecFirst) ||
            string.IsNullOrWhiteSpace(ecLast) ||
            string.IsNullOrWhiteSpace(ecRel) ||
            string.IsNullOrWhiteSpace(ecEmail) ||
            !ecEmail.Contains('@') ||
            string.IsNullOrWhiteSpace(ecPhone))
            return null;

        // Holds the optional linked user account ID if a login account is created.
        Guid? userId = null;

        // Optionally create a linked login account for the driver.
        if (command.CreateLoginAccount)
        {
            // Password is required when creating a login-enabled driver.
            if (string.IsNullOrWhiteSpace(command.Password))
                return null;

            // Enforce password policy.
            if (!PasswordPolicy.IsValid(command.Password, out _))
                return null;

            // Ensure the email address is not already in use.
            var existing = await _users.GetByEmailAsync(email);
            if (existing != null)
                return null;

            // Hash password before storing.
            var hash = PasswordHasher.Hash(command.Password);

            // Create a sub-user account under the owner/main user.
            var subUser = UserAccount.CreateSubUser(
                parentMainUserId: command.OwnerUserId,
                firstName: firstName!,
                lastName: lastName!,
                email: email!,
                passwordHash: hash
            );

            await _users.AddAsync(subUser);

            // Link the created driver to the new user account.
            userId = subUser.Id;
        }

        // Create the driver entity, optionally linked to a login account.
        var driver = new Driver(
            ownerUserId: command.OwnerUserId,
            userId: userId,
            firstName: firstName!,
            lastName: lastName!,
            email: email!
        );

        // Apply licence details.
        driver.UpdateLicenceNumber(command.LicenceNumber);
        driver.UpdateLicenceVersion(command.LicenceVersion);
        driver.UpdateLicenceClassOrEndorsements(command.LicenceClassOrEndorsements);
        driver.UpdateLicenceIssuedOnUtc(command.LicenceIssuedOnUtc);
        driver.UpdateLicenceConditionsNotes(command.LicenceConditionsNotes);

        // Apply contact and profile details.
        driver.UpdatePhone(command.PhoneNumber);
        driver.UpdateDateOfBirthUtc(command.DateOfBirthUtc);
        driver.UpdateLicenceExpiryUtc(command.LicenceExpiresOnUtc);

        // Apply address details.
        driver.UpdateAddress(
            command.Line1,
            command.Suburb,
            command.City,
            command.Region,
            command.Postcode,
            command.Country
        );

        // Apply emergency contact details.
        driver.UpdateEmergencyContact(new EmergencyContact(
            ecFirst!, ecLast!, ecRel!, ecEmail!, ecPhone!, ecPhone2
        ));

        // Persist the driver entity.
        await _repository.SaveAsync(driver);

        // Ensure required induction/compliance records exist for this driver.
        await _complianceEnsurer.EnsureDriverInductionsExistForDriverAsync(
            command.OwnerUserId,
            driver.Id
        );

        return driver;
    }
}