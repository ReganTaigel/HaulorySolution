using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Limits;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Drivers;

// Handles the application use case for creating a driver profile
// from an existing user account.
public class CreateDriverFromUserHandler
{
    // Repository used to load and persist driver entities.
    private readonly IDriverRepository _repository;

    // Ensures required induction/compliance records exist after driver creation.
    private readonly IComplianceEnsurer _complianceEnsurer;

    // Repository used to load the existing user account.
    private readonly IUserAccountRepository _users;

    // Constructor injection of dependencies.
    public CreateDriverFromUserHandler(
        IDriverRepository repository,
        IComplianceEnsurer complianceEnsurer,
        IUserAccountRepository users)
    {
        _repository = repository;
        _complianceEnsurer = complianceEnsurer;
        _users = users;
    }

    // Creates a driver profile linked to an existing user account.
    public async Task<Driver?> HandleAsync(CreateDriverFromUserCommand command)
    {
        // Reject invalid user IDs.
        if (command.UserId == Guid.Empty)
            return null;

        // Normalise incoming identity values.
        var email = command.Email?.Trim().ToLowerInvariant();
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();

        // Validate required identity fields.
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email))
            return null;

        // Load the existing user account.
        var actor = await _users.GetByIdAsync(command.UserId);
        if (actor == null)
            return null;

        // Determine the owner account:
        // - main users own themselves
        // - sub-users belong to their parent main user
        var ownerUserId = actor.Role == UserRole.Main
            ? actor.Id
            : (actor.ParentMainUserId ?? Guid.Empty);

        if (ownerUserId == Guid.Empty)
            return null;

        // Prevent duplicate driver creation for the same user account.
        var existing = await _repository.GetByUserIdAsync(actor.Id);
        if (existing != null)
            return existing;

        // Enforce plan limits differently for main users vs sub-users.
        if (actor.Role == UserRole.Main)
        {
            var mainCount = await _repository.CountMainDriversAsync(ownerUserId);
            if (mainCount >= PlanLimits.MaxMainDrivers)
                throw new InvalidOperationException("Main driver limit reached (max 1).");
        }
        else
        {
            var subCount = await _repository.CountSubDriversAsync(ownerUserId);
            if (subCount >= PlanLimits.MaxSubDrivers)
                throw new InvalidOperationException("Sub driver limit reached.");
        }

        // Create the driver entity linked directly to the existing user account.
        var driver = new Driver(
            ownerUserId: ownerUserId,
            userId: actor.Id,
            firstName: firstName!,
            lastName: lastName!,
            email: email!
        );

        // Apply contact and personal details.
        driver.UpdatePhone(command.PhoneNumber);
        driver.UpdateDateOfBirthUtc(command.DateOfBirthUtc);

        // Apply licence details.
        driver.UpdateLicenceNumber(command.LicenceNumber);
        driver.UpdateLicenceVersion(command.LicenceVersion);
        driver.UpdateLicenceClassOrEndorsements(command.LicenceClassOrEndorsements);
        driver.UpdateLicenceIssuedOnUtc(command.LicenceIssuedOnUtc);
        driver.UpdateLicenceExpiryUtc(command.LicenceExpiresOnUtc);
        driver.UpdateLicenceConditionsNotes(command.LicenceConditionsNotes);

        // Apply address details.
        driver.UpdateAddress(
            line1: command.Line1,
            line2: command.Line2,
            suburb: command.Suburb,
            city: command.City,
            region: command.Region,
            postcode: command.Postcode,
            country: command.Country
        );

        // Persist the driver entity.
        await _repository.SaveAsync(driver);

        // Ensure required induction/compliance records exist for this driver.
        await _complianceEnsurer
            .EnsureDriverInductionsExistForDriverAsync(ownerUserId, driver.Id);

        return driver;
    }
}