namespace Haulory.Application.Features.Drivers;

public record CreateDriverFromUserCommand(
    Guid UserId,

    string FirstName,
    string LastName,
    string Email,

    string? PhoneNumber,
    DateTime? DateOfBirthUtc,

    string? LicenceNumber,
    string? LicenceVersion,
    string? LicenceClassOrEndorsements,
    DateTime? LicenceIssuedOnUtc,
    DateTime? LicenceExpiresOnUtc,
    string? LicenceConditionsNotes,

    string? Line1,
    string? Line2,
    string? Suburb,
    string? City,
    string? Region,
    string? Postcode,
    string? Country
);