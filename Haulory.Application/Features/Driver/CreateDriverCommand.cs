namespace Haulory.Application.Features.Drivers;

public record CreateDriverCommand(
    Guid OwnerUserId,
    string FirstName,
    string LastName,
    string Email,
    string? LicenceNumber,

    // Emergency Contact (required)
    string EmergencyFirstName,
    string EmergencyLastName,
    string EmergencyRelationship,
    string EmergencyEmail,
    string EmergencyPhoneNumber,
    string? EmergencySecondaryPhoneNumber
);
