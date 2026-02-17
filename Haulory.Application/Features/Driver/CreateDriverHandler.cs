using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Drivers;

public class CreateDriverHandler
{
    private readonly IDriverRepository _repository;
    private readonly IComplianceEnsurer _complianceEnsurer;

    public CreateDriverHandler(IDriverRepository repository, IComplianceEnsurer complianceEnsurer)
    {
        _repository = repository;
        _complianceEnsurer = complianceEnsurer;
    }

    public async Task<Driver?> HandleAsync(CreateDriverCommand command)
    {
        if (command.OwnerUserId == Guid.Empty)
            return null;

        var firstName = command.FirstName?.Trim().ToUpper();
        var lastName = command.LastName?.Trim().ToUpper();
        var email = command.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            !email.Contains('@'))
            return null;

        // Emergency Contact required (your current rule)
        var ecFirst = command.EmergencyFirstName?.Trim().ToUpper();
        var ecLast = command.EmergencyLastName?.Trim().ToUpper();
        var ecRel = command.EmergencyRelationship?.Trim();
        var ecEmail = command.EmergencyEmail?.Trim().ToLowerInvariant();
        var ecPhone = command.EmergencyPhoneNumber?.Trim();

        var ecPhone2 = string.IsNullOrWhiteSpace(command.EmergencySecondaryPhoneNumber)
            ? null
            : command.EmergencySecondaryPhoneNumber.Trim();

        if (string.IsNullOrWhiteSpace(ecFirst) ||
            string.IsNullOrWhiteSpace(ecLast) ||
            string.IsNullOrWhiteSpace(ecRel) ||
            string.IsNullOrWhiteSpace(ecEmail) ||
            !ecEmail.Contains('@') ||
            string.IsNullOrWhiteSpace(ecPhone))
            return null;

        // SUB driver: UserId = null, OwnerUserId = command.OwnerUserId
        var driver = new Driver(
            ownerUserId: command.OwnerUserId,
            userId: null,
            firstName: firstName!,
            lastName: lastName!,
            email: email!
        );

        // Existing
        driver.UpdateLicenceNumber(command.LicenceNumber);

        // NEW fields
        driver.UpdatePhone(command.PhoneNumber);
        driver.UpdateDateOfBirthUtc(command.DateOfBirthUtc);
        driver.UpdateLicenceExpiryUtc(command.LicenceExpiresOnUtc);

        driver.UpdateAddress(
            command.Line1,
            command.Line2,
            command.Suburb,
            command.City,
            command.Region,
            command.Postcode,
            command.Country
        );

        driver.UpdateEmergencyContact(new EmergencyContact(
            ecFirst!, ecLast!, ecRel!, ecEmail!, ecPhone!, ecPhone2
        ));

        await _repository.SaveAsync(driver);

        // auto-create induction/compliance rows for this driver
        await _complianceEnsurer.EnsureDriverInductionsExistForDriverAsync(command.OwnerUserId, driver.Id);

        return driver;
    }
}
