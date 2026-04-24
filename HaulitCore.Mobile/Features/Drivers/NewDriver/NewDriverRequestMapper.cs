using HaulitCore.Contracts.Drivers;

namespace HaulitCore.Mobile.Features.Drivers.NewDriver;

// Maps the driver form state from the mobile UI into API request models.
// Centralises trimming, null handling, and date conversion logic.
public sealed class NewDriverRequestMapper
{
    // Maps form state into a CreateDriverRequest for new driver creation.
    public CreateDriverRequest MapCreate(NewDriverFormState state)
        => new()
        {
            // Core identity fields.
            FirstName = state.FirstName.Trim(),
            LastName = state.LastName.Trim(),
            Email = state.Email.Trim().ToLowerInvariant(),

            // Optional contact and profile fields.
            PhoneNumber = NullIfWhiteSpace(state.PhoneNumber),
            DateOfBirthUtc = ToUtcDate(state.DateOfBirthLocal),

            // Licence details.
            LicenceNumber = NullIfWhiteSpace(state.LicenceNumber),
            LicenceVersion = NullIfWhiteSpace(state.LicenceVersion),
            LicenceClassOrEndorsements = NullIfWhiteSpace(state.LicenceClassOrEndorsements),
            LicenceIssuedOnUtc = ToUtcDate(state.LicenceIssuedLocal),
            LicenceExpiresOnUtc = ToUtcDate(state.LicenceExpiryLocal),
            LicenceConditionsNotes = NullIfWhiteSpace(state.LicenceConditionsNotes),

            // Address details.
            Line1 = NullIfWhiteSpace(state.Line1),
            Suburb = NullIfWhiteSpace(state.Suburb),
            City = NullIfWhiteSpace(state.City),
            Region = NullIfWhiteSpace(state.Region),
            Postcode = NullIfWhiteSpace(state.Postcode),
            Country = NullIfWhiteSpace(state.Country),

            // Emergency contact details.
            EmergencyContact = BuildEmergencyContact(state),

            // Optional login account creation for the new driver.
            CreateLoginAccount = state.CreateLoginAccount,
            Password = string.IsNullOrWhiteSpace(state.Password) ? null : state.Password
        };

    // Maps form state into an UpdateDriverRequest for editing an existing driver.
    public UpdateDriverRequest MapUpdate(NewDriverFormState state)
        => new()
        {
            // Core identity fields.
            FirstName = state.FirstName.Trim(),
            LastName = state.LastName.Trim(),
            Email = state.Email.Trim().ToLowerInvariant(),

            // Optional contact and profile fields.
            PhoneNumber = NullIfWhiteSpace(state.PhoneNumber),
            DateOfBirthUtc = ToUtcDate(state.DateOfBirthLocal),

            // Licence details.
            LicenceNumber = NullIfWhiteSpace(state.LicenceNumber),
            LicenceVersion = NullIfWhiteSpace(state.LicenceVersion),
            LicenceClassOrEndorsements = NullIfWhiteSpace(state.LicenceClassOrEndorsements),
            LicenceIssuedOnUtc = ToUtcDate(state.LicenceIssuedLocal),
            LicenceExpiresOnUtc = ToUtcDate(state.LicenceExpiryLocal),
            LicenceConditionsNotes = NullIfWhiteSpace(state.LicenceConditionsNotes),

            // Address details.
            Line1 = NullIfWhiteSpace(state.Line1),
            Line2 = NullIfWhiteSpace(state.Line2),
            Suburb = NullIfWhiteSpace(state.Suburb),
            City = NullIfWhiteSpace(state.City),
            Region = NullIfWhiteSpace(state.Region),
            Postcode = NullIfWhiteSpace(state.Postcode),
            Country = NullIfWhiteSpace(state.Country),

            // Emergency contact details.
            EmergencyContact = BuildEmergencyContact(state)
        };

    // Builds the nested emergency contact request from form state.
    private static EmergencyContactRequest BuildEmergencyContact(NewDriverFormState state)
        => new()
        {
            FirstName = state.EmergencyFirstName.Trim(),
            LastName = state.EmergencyLastName.Trim(),
            Relationship = state.EmergencyRelationship.Trim(),
            Email = state.EmergencyEmail.Trim().ToLowerInvariant(),
            PhoneNumber = state.EmergencyPhoneNumber.Trim(),
            SecondaryPhoneNumber = NullIfWhiteSpace(state.EmergencySecondaryPhoneNumber)
        };

    // Converts blank or whitespace strings to null, otherwise returns a trimmed value.
    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    // Converts a local date value into UTC, preserving only the date component.
    private static DateTime ToUtcDate(DateTime localDate)
        => DateTime.SpecifyKind(localDate.Date, DateTimeKind.Local).ToUniversalTime();
}