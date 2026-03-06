public record CreateDriverCommand(
    Guid OwnerUserId,
    string FirstName,
    string LastName,
    string Email,
    string? LicenceNumber,

    // Contact + profile
    string? PhoneNumber,
    DateTime? DateOfBirthUtc,

    // Licence
    DateTime? LicenceExpiresOnUtc,
    string? LicenceVersion,
    string? LicenceClassOrEndorsements,
    DateTime? LicenceIssuedOnUtc,
    string? LicenceConditionsNotes,

    // Address
    string? Line1,
    string? Line2,
    string? Suburb,
    string? City,
    string? Region,
    string? Postcode,
    string? Country,

    // Emergency Contact
    string EmergencyFirstName,
    string EmergencyLastName,
    string EmergencyRelationship,
    string EmergencyEmail,
    string EmergencyPhoneNumber,
    string? EmergencySecondaryPhoneNumber,

    // ✅ NEW (for login driver accounts)
    bool CreateLoginAccount,
    string? Password
);