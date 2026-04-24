namespace HaulitCore.Application.Features.Drivers;

// Represents a command to create a driver profile from an existing user account.
// Used when a user already exists and needs to be linked as a driver.
public record CreateDriverFromUserCommand(
    // Existing user account ID to associate with the driver.
    Guid UserId,

    // Core identity details.
    string FirstName,
    string LastName,
    string Email,

    // Contact and personal details.
    string? PhoneNumber,
    DateTime? DateOfBirthUtc,

    // Licence information.
    string? LicenceNumber,
    string? LicenceVersion,
    string? LicenceClassOrEndorsements,
    DateTime? LicenceIssuedOnUtc,
    DateTime? LicenceExpiresOnUtc,
    string? LicenceConditionsNotes,

    // Address details.
    string? Line1,
    string? Suburb,
    string? City,
    string? Region,
    string? Postcode,
    string? Country
);