// Application command used to create a new driver.
// Carries all data required by the driver creation workflow.
public record CreateDriverCommand(
    // Owner/business account the driver belongs to.
    Guid OwnerUserId,

    // Core identity details.
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

    // Optional login account creation for drivers who need direct system access.
    bool CreateLoginAccount,
    string? Password
);