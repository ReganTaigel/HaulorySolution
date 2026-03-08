namespace Haulory.Mobile.Contracts.Drivers;

public sealed class DriverDto
{
    public Guid Id { get; set; }
    public Guid OwnerUserId { get; set; }
    public Guid? UserId { get; set; }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? DisplayName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public DateTime? DateOfBirthUtc { get; set; }

    public string? LicenceNumber { get; set; }
    public string? LicenceVersion { get; set; }
    public string? LicenceClassOrEndorsements { get; set; }
    public DateTime? LicenceIssuedOnUtc { get; set; }
    public DateTime? LicenceExpiresOnUtc { get; set; }
    public string? LicenceConditionsNotes { get; set; }

    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Suburb { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }
    public string? AddressSummary { get; set; }

    public string? Status { get; set; }

    public EmergencyContactDto EmergencyContact { get; set; } = new();
    public string? EmergencyStatus { get; set; }

    public bool IsMainProfile { get; set; }
}