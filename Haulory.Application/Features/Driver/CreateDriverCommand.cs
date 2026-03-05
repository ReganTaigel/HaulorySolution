namespace Haulory.Application.Features.Drivers;

public record CreateDriverCommand(
    Guid OwnerUserId,
    string FirstName,
    string LastName,
    string Email,
    string? LicenceNumber,

    // NEW: Contact + profile
    string? PhoneNumber,
    DateTime? DateOfBirthUtc,

    // NEW: Licence
    DateTime? LicenceExpiresOnUtc,
    string? LicenceVersion,                  
    string? LicenceClassOrEndorsements,    
    DateTime? LicenceIssuedOnUtc,          
    string? LicenceConditionsNotes,         

    // NEW: Address
    string? Line1,
    string? Line2,
    string? Suburb,
    string? City,
    string? Region,
    string? Postcode,
    string? Country,

    // Emergency Contact (required)
    string EmergencyFirstName,
    string EmergencyLastName,
    string EmergencyRelationship,
    string EmergencyEmail,
    string EmergencyPhoneNumber,
    string? EmergencySecondaryPhoneNumber
);
