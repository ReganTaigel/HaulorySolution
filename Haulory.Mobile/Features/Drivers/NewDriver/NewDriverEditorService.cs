using Haulory.Mobile.Services;

namespace Haulory.Mobile.Features.Drivers.NewDriver;

// Handles loading and saving driver data between the API and UI form state.
// Acts as an orchestration layer between the API service and the form state model.
public sealed class NewDriverEditorService
{
    // API service for driver operations (create, update, fetch).
    private readonly DriversApiService _driversApiService;

    // Mapper for converting UI state into API request models.
    private readonly NewDriverRequestMapper _mapper;

    public NewDriverEditorService(
        DriversApiService driversApiService,
        NewDriverRequestMapper mapper)
    {
        _driversApiService = driversApiService;
        _mapper = mapper;
    }

    // Loads a driver from the API and populates the form state for editing.
    public async Task LoadIntoStateAsync(NewDriverFormState state, Guid driverId)
    {
        // Fetch driver data from API.
        var driver = await _driversApiService.GetDriverByIdAsync(driverId);
        if (driver == null)
            throw new InvalidOperationException("Driver could not be loaded.");

        // Populate identity and contact fields.
        state.FirstName = driver.FirstName ?? string.Empty;
        state.LastName = driver.LastName ?? string.Empty;
        state.Email = driver.Email ?? string.Empty;
        state.PhoneNumber = driver.PhoneNumber ?? string.Empty;

        // Convert UTC date of birth to local date for UI display.
        if (driver.DateOfBirthUtc.HasValue)
            state.DateOfBirthLocal = driver.DateOfBirthUtc.Value.ToLocalTime().Date;

        // Populate licence details.
        state.LicenceNumber = driver.LicenceNumber ?? string.Empty;
        state.LicenceVersion = driver.LicenceVersion ?? string.Empty;
        state.LicenceClassOrEndorsements = driver.LicenceClassOrEndorsements ?? string.Empty;
        state.LicenceConditionsNotes = driver.LicenceConditionsNotes ?? string.Empty;

        // Convert licence issue date to local date.
        if (driver.LicenceIssuedOnUtc.HasValue)
            state.LicenceIssuedLocal = driver.LicenceIssuedOnUtc.Value.ToLocalTime().Date;

        // Convert licence expiry date to local date.
        if (driver.LicenceExpiresOnUtc.HasValue)
            state.LicenceExpiryLocal = driver.LicenceExpiresOnUtc.Value.ToLocalTime().Date;

        // Populate address fields.
        state.Line1 = driver.Line1 ?? string.Empty;
        state.Line2 = driver.Line2 ?? string.Empty;
        state.Suburb = driver.Suburb ?? string.Empty;
        state.City = driver.City ?? string.Empty;
        state.Region = driver.Region ?? string.Empty;
        state.Postcode = driver.Postcode ?? string.Empty;
        state.Country = driver.Country ?? string.Empty;

        // Populate emergency contact details.
        state.EmergencyFirstName = driver.EmergencyContact?.FirstName ?? string.Empty;
        state.EmergencyLastName = driver.EmergencyContact?.LastName ?? string.Empty;
        state.EmergencyRelationship = driver.EmergencyContact?.Relationship ?? string.Empty;
        state.EmergencyEmail = driver.EmergencyContact?.Email ?? string.Empty;
        state.EmergencyPhoneNumber = driver.EmergencyContact?.PhoneNumber ?? string.Empty;
        state.EmergencySecondaryPhoneNumber = driver.EmergencyContact?.SecondaryPhoneNumber ?? string.Empty;

        // Reset login account fields (not editable in edit mode).
        state.CreateLoginAccount = false;
        state.Password = string.Empty;
    }

    // Saves the form state by either creating or updating a driver.
    public Task SaveAsync(NewDriverFormState state)
    {
        // If editing an existing driver, call update endpoint.
        if (state.IsEditMode && Guid.TryParse(state.DriverId, out var driverId))
            return _driversApiService.UpdateDriverAsync(driverId, _mapper.MapUpdate(state));

        // Otherwise, create a new driver.
        return _driversApiService.CreateDriverAsync(_mapper.MapCreate(state));
    }
}