using Haulory.Contracts.Drivers;

namespace Haulory.Mobile.Features.Drivers.NewDriver;

public sealed class NewDriverRequestMapper
{
    public CreateDriverRequest MapCreate(NewDriverFormState state)
        => new()
        {
            FirstName = state.FirstName.Trim(),
            LastName = state.LastName.Trim(),
            Email = state.Email.Trim().ToLowerInvariant(),
            PhoneNumber = NullIfWhiteSpace(state.PhoneNumber),
            DateOfBirthUtc = ToUtcDate(state.DateOfBirthLocal),
            LicenceNumber = NullIfWhiteSpace(state.LicenceNumber),
            LicenceVersion = NullIfWhiteSpace(state.LicenceVersion),
            LicenceClassOrEndorsements = NullIfWhiteSpace(state.LicenceClassOrEndorsements),
            LicenceIssuedOnUtc = ToUtcDate(state.LicenceIssuedLocal),
            LicenceExpiresOnUtc = ToUtcDate(state.LicenceExpiryLocal),
            LicenceConditionsNotes = NullIfWhiteSpace(state.LicenceConditionsNotes),
            Line1 = NullIfWhiteSpace(state.Line1),
            Line2 = NullIfWhiteSpace(state.Line2),
            Suburb = NullIfWhiteSpace(state.Suburb),
            City = NullIfWhiteSpace(state.City),
            Region = NullIfWhiteSpace(state.Region),
            Postcode = NullIfWhiteSpace(state.Postcode),
            Country = NullIfWhiteSpace(state.Country),
            EmergencyContact = BuildEmergencyContact(state),
            CreateLoginAccount = state.CreateLoginAccount,
            Password = string.IsNullOrWhiteSpace(state.Password) ? null : state.Password
        };

    public UpdateDriverRequest MapUpdate(NewDriverFormState state)
        => new()
        {
            FirstName = state.FirstName.Trim(),
            LastName = state.LastName.Trim(),
            Email = state.Email.Trim().ToLowerInvariant(),
            PhoneNumber = NullIfWhiteSpace(state.PhoneNumber),
            DateOfBirthUtc = ToUtcDate(state.DateOfBirthLocal),
            LicenceNumber = NullIfWhiteSpace(state.LicenceNumber),
            LicenceVersion = NullIfWhiteSpace(state.LicenceVersion),
            LicenceClassOrEndorsements = NullIfWhiteSpace(state.LicenceClassOrEndorsements),
            LicenceIssuedOnUtc = ToUtcDate(state.LicenceIssuedLocal),
            LicenceExpiresOnUtc = ToUtcDate(state.LicenceExpiryLocal),
            LicenceConditionsNotes = NullIfWhiteSpace(state.LicenceConditionsNotes),
            Line1 = NullIfWhiteSpace(state.Line1),
            Line2 = NullIfWhiteSpace(state.Line2),
            Suburb = NullIfWhiteSpace(state.Suburb),
            City = NullIfWhiteSpace(state.City),
            Region = NullIfWhiteSpace(state.Region),
            Postcode = NullIfWhiteSpace(state.Postcode),
            Country = NullIfWhiteSpace(state.Country),
            EmergencyContact = BuildEmergencyContact(state)
        };

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

    private static string? NullIfWhiteSpace(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static DateTime ToUtcDate(DateTime localDate)
        => DateTime.SpecifyKind(localDate.Date, DateTimeKind.Local).ToUniversalTime();
}
