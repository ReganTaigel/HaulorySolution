using Haulory.Mobile.Services;

namespace Haulory.Mobile.Features.Drivers.NewDriver;

public sealed class NewDriverEditorService
{
    private readonly DriversApiService _driversApiService;
    private readonly NewDriverRequestMapper _mapper;

    public NewDriverEditorService(
        DriversApiService driversApiService,
        NewDriverRequestMapper mapper)
    {
        _driversApiService = driversApiService;
        _mapper = mapper;
    }

    public async Task LoadIntoStateAsync(NewDriverFormState state, Guid driverId)
    {
        var driver = await _driversApiService.GetDriverByIdAsync(driverId);
        if (driver == null)
            throw new InvalidOperationException("Driver could not be loaded.");

        state.FirstName = driver.FirstName ?? string.Empty;
        state.LastName = driver.LastName ?? string.Empty;
        state.Email = driver.Email ?? string.Empty;
        state.PhoneNumber = driver.PhoneNumber ?? string.Empty;

        if (driver.DateOfBirthUtc.HasValue)
            state.DateOfBirthLocal = driver.DateOfBirthUtc.Value.ToLocalTime().Date;

        state.LicenceNumber = driver.LicenceNumber ?? string.Empty;
        state.LicenceVersion = driver.LicenceVersion ?? string.Empty;
        state.LicenceClassOrEndorsements = driver.LicenceClassOrEndorsements ?? string.Empty;
        state.LicenceConditionsNotes = driver.LicenceConditionsNotes ?? string.Empty;

        if (driver.LicenceIssuedOnUtc.HasValue)
            state.LicenceIssuedLocal = driver.LicenceIssuedOnUtc.Value.ToLocalTime().Date;

        if (driver.LicenceExpiresOnUtc.HasValue)
            state.LicenceExpiryLocal = driver.LicenceExpiresOnUtc.Value.ToLocalTime().Date;

        state.Line1 = driver.Line1 ?? string.Empty;
        state.Line2 = driver.Line2 ?? string.Empty;
        state.Suburb = driver.Suburb ?? string.Empty;
        state.City = driver.City ?? string.Empty;
        state.Region = driver.Region ?? string.Empty;
        state.Postcode = driver.Postcode ?? string.Empty;
        state.Country = driver.Country ?? string.Empty;

        state.EmergencyFirstName = driver.EmergencyContact?.FirstName ?? string.Empty;
        state.EmergencyLastName = driver.EmergencyContact?.LastName ?? string.Empty;
        state.EmergencyRelationship = driver.EmergencyContact?.Relationship ?? string.Empty;
        state.EmergencyEmail = driver.EmergencyContact?.Email ?? string.Empty;
        state.EmergencyPhoneNumber = driver.EmergencyContact?.PhoneNumber ?? string.Empty;
        state.EmergencySecondaryPhoneNumber = driver.EmergencyContact?.SecondaryPhoneNumber ?? string.Empty;

        state.CreateLoginAccount = false;
        state.Password = string.Empty;
    }

    public Task SaveAsync(NewDriverFormState state)
    {
        if (state.IsEditMode && Guid.TryParse(state.DriverId, out var driverId))
            return _driversApiService.UpdateDriverAsync(driverId, _mapper.MapUpdate(state));

        return _driversApiService.CreateDriverAsync(_mapper.MapCreate(state));
    }
}
