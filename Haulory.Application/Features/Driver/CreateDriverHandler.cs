using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Limits;
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

        // ✅ LIMIT: 4 sub drivers max
        var subCount = await _repository.CountSubDriversAsync(command.OwnerUserId);
        if (subCount >= PlanLimits.MaxSubDrivers)
            throw new InvalidOperationException("Sub driver limit reached (max 4).");

        var firstName = command.FirstName?.Trim();
        var lastName = command.LastName?.Trim();
        var email = command.Email?.Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(firstName) ||
            string.IsNullOrWhiteSpace(lastName) ||
            string.IsNullOrWhiteSpace(email) ||
            !email.Contains('@'))
            return null;

        var ecFirst = command.EmergencyFirstName?.Trim();
        var ecLast = command.EmergencyLastName?.Trim();
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

        var driver = new Driver(
            ownerUserId: command.OwnerUserId,
            userId: null,
            firstName: firstName!,
            lastName: lastName!,
            email: email!
        );

        driver.UpdateLicenceNumber(command.LicenceNumber);
        driver.UpdateLicenceVersion(command.LicenceVersion);
        driver.UpdateLicenceClassOrEndorsements(command.LicenceClassOrEndorsements);
        driver.UpdateLicenceIssuedOnUtc(command.LicenceIssuedOnUtc);
        driver.UpdateLicenceConditionsNotes(command.LicenceConditionsNotes);

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

        await _complianceEnsurer.EnsureDriverInductionsExistForDriverAsync(
            command.OwnerUserId,
            driver.Id
        );

        return driver;
    }
}