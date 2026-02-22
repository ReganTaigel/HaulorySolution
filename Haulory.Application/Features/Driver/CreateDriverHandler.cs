using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Drivers;

public class CreateDriverHandler
{
    #region Dependencies

    private readonly IDriverRepository _repository;
    private readonly IComplianceEnsurer _complianceEnsurer;

    #endregion

    #region Constructor

    public CreateDriverHandler(IDriverRepository repository, IComplianceEnsurer complianceEnsurer)
    {
        _repository = repository;
        _complianceEnsurer = complianceEnsurer;
    }

    #endregion

    #region Public API

    // Creates a sub-driver for an owner user (UserId is null, OwnerUserId is required),
    // persists it, then ensures default induction/compliance records exist.
    // <param name="command">Input data for driver creation.</param>
    // <returns>The created driver, or null if validation fails.</returns>
    public async Task<Driver?> HandleAsync(CreateDriverCommand command)
    {
        // Basic required identifier
        if (command.OwnerUserId == Guid.Empty)
            return null;

        // Normalize primary driver fields (keeps data consistent across storage & comparisons)
        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();
        var email = command.Email?.Trim().ToLowerInvariant();

        // Minimal validation (kept intentionally lightweight; caller can enforce stronger rules)
        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            !email.Contains('@'))
            return null;

        // Emergency contact is required (current business rule)
        var ecFirst = command.EmergencyFirstName?.Trim();
        var ecLast = command.EmergencyLastName?.Trim();
        var ecRel = command.EmergencyRelationship?.Trim();
        var ecEmail = command.EmergencyEmail?.Trim().ToLowerInvariant();
        var ecPhone = command.EmergencyPhoneNumber?.Trim();

        // Optional secondary phone
        var ecPhone2 = string.IsNullOrWhiteSpace(command.EmergencySecondaryPhoneNumber)
            ? null
            : command.EmergencySecondaryPhoneNumber.Trim();

        // Validate emergency contact minimums
        if (string.IsNullOrWhiteSpace(ecFirst) ||
            string.IsNullOrWhiteSpace(ecLast) ||
            string.IsNullOrWhiteSpace(ecRel) ||
            string.IsNullOrWhiteSpace(ecEmail) ||
            !ecEmail.Contains('@') ||
            string.IsNullOrWhiteSpace(ecPhone))
            return null;

        // SUB driver model:
        // - OwnerUserId is the owning customer/company user
        // - UserId is null because this driver is not a login account
        var driver = new Driver(
            ownerUserId: command.OwnerUserId,
            userId: null,
            firstName: firstName!,
            lastName: lastName!,
            email: email!
        );

        // Populate optional driver fields via domain methods (keeps invariants inside the entity)
        driver.UpdateLicenceNumber(command.LicenceNumber);
        driver.UpdatePhone(command.PhoneNumber);
        driver.UpdateDateOfBirthUtc(command.DateOfBirthUtc);
        driver.UpdateLicenceExpiryUtc(command.LicenceExpiresOnUtc);

        // Address is optional; pass through and let the entity decide how to store/validate
        driver.UpdateAddress(
            command.Line1,
            command.Line2,
            command.Suburb,
            command.City,
            command.Region,
            command.Postcode,
            command.Country
        );

        // Emergency contact gets stored as a value object/entity in the Driver aggregate
        driver.UpdateEmergencyContact(new EmergencyContact(
            ecFirst!, ecLast!, ecRel!, ecEmail!, ecPhone!, ecPhone2
        ));

        // Persist driver first so it has an Id for downstream setup
        await _repository.SaveAsync(driver);

        // Automatically seed induction/compliance rows for the new driver
        await _complianceEnsurer.EnsureDriverInductionsExistForDriverAsync(
            command.OwnerUserId,
            driver.Id
        );

        return driver;
    }

    #endregion
}
