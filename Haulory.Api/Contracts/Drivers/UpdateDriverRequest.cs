namespace Haulory.Api.Contracts.Drivers;

public sealed class UpdateDriverRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

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

    public EmergencyContactRequest? EmergencyContact { get; set; }
}