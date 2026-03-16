using Haulory.Application.Interfaces.Repositories;
using Haulory.Contracts.Drivers;
using Haulory.Domain.Entities;

namespace Haulory.Api.Drivers;

public sealed class DriverWorkflowService
{
    private readonly IDriverRepository _driverRepository;

    public DriverWorkflowService(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    public async Task<Driver?> UpdateAsync(Guid ownerUserId, Guid id, UpdateDriverRequest request)
    {
        var driver = await _driverRepository.GetByIdForOwnerAsync(ownerUserId, id);
        if (driver is null)
            return null;

        driver.UpdateIdentity(request.FirstName, request.LastName, request.Email);
        driver.UpdatePhone(request.PhoneNumber);
        driver.UpdateDateOfBirthUtc(request.DateOfBirthUtc);
        driver.UpdateLicenceNumber(request.LicenceNumber);
        driver.UpdateLicenceVersion(request.LicenceVersion);
        driver.UpdateLicenceClassOrEndorsements(request.LicenceClassOrEndorsements);
        driver.UpdateLicenceIssuedOnUtc(request.LicenceIssuedOnUtc);
        driver.UpdateLicenceExpiryUtc(request.LicenceExpiresOnUtc);
        driver.UpdateLicenceConditionsNotes(request.LicenceConditionsNotes);
        driver.UpdateAddress(
            request.Line1,
            request.Line2,
            request.Suburb,
            request.City,
            request.Region,
            request.Postcode,
            request.Country);

        driver.UpdateEmergencyContact(new EmergencyContact(
            request.EmergencyContact?.FirstName ?? string.Empty,
            request.EmergencyContact?.LastName ?? string.Empty,
            request.EmergencyContact?.Relationship ?? string.Empty,
            request.EmergencyContact?.Email ?? string.Empty,
            request.EmergencyContact?.PhoneNumber ?? string.Empty,
            request.EmergencyContact?.SecondaryPhoneNumber));

        await _driverRepository.SaveAsync(driver);
        return driver;
    }
}
