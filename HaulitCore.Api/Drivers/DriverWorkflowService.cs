using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Contracts.Drivers;
using HaulitCore.Domain.Entities;

namespace HaulitCore.Api.Drivers;

// Encapsulates business logic for driver-related workflows.
// Keeps controllers thin by centralising update operations.
public sealed class DriverWorkflowService
{
    // Repository used to load and persist driver entities.
    private readonly IDriverRepository _driverRepository;

    // Constructor injection of the repository.
    public DriverWorkflowService(IDriverRepository driverRepository)
    {
        _driverRepository = driverRepository;
    }

    // Updates an existing driver record for a given owner.
    public async Task<Driver?> UpdateAsync(Guid ownerUserId, Guid id, UpdateDriverRequest request)
    {
        // Retrieve the driver scoped to the owner to enforce multi-tenant safety.
        var driver = await _driverRepository.GetByIdForOwnerAsync(ownerUserId, id);

        // Return null if the driver does not exist or does not belong to the owner.
        if (driver is null)
            return null;

        // Update core identity fields.
        driver.UpdateIdentity(request.FirstName, request.LastName, request.Email);

        // Update contact and personal details.
        driver.UpdatePhone(request.PhoneNumber);
        driver.UpdateDateOfBirthUtc(request.DateOfBirthUtc);

        // Update licence-related details.
        driver.UpdateLicenceNumber(request.LicenceNumber);
        driver.UpdateLicenceVersion(request.LicenceVersion);
        driver.UpdateLicenceClassOrEndorsements(request.LicenceClassOrEndorsements);
        driver.UpdateLicenceIssuedOnUtc(request.LicenceIssuedOnUtc);
        driver.UpdateLicenceExpiryUtc(request.LicenceExpiresOnUtc);
        driver.UpdateLicenceConditionsNotes(request.LicenceConditionsNotes);

        // Update address details.
        driver.UpdateAddress(
            request.Line1,
            request.Suburb,
            request.City,
            request.Region,
            request.Postcode,
            request.Country);

        // Update emergency contact information.
        // Defaults to empty strings if any fields are missing.
        driver.UpdateEmergencyContact(new EmergencyContact(
            request.EmergencyContact?.FirstName ?? string.Empty,
            request.EmergencyContact?.LastName ?? string.Empty,
            request.EmergencyContact?.Relationship ?? string.Empty,
            request.EmergencyContact?.Email ?? string.Empty,
            request.EmergencyContact?.PhoneNumber ?? string.Empty,
            request.EmergencyContact?.SecondaryPhoneNumber));

        // Persist all changes to the data store.
        await _driverRepository.SaveAsync(driver);

        // Return the updated driver entity.
        return driver;
    }
}