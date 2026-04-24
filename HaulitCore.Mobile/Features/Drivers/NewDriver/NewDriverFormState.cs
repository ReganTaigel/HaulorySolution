namespace HaulitCore.Mobile.Features.Drivers.NewDriver;

public sealed class NewDriverFormState
{
    public string DriverId { get; set; } = string.Empty;
    public bool IsSaving { get; set; }
    public bool IsMainProfile { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string LicenceNumber { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public DateTime DateOfBirthLocal { get; set; } = DateTime.Today;
    public DateTime LicenceExpiryLocal { get; set; } = DateTime.Today;
    public string LicenceVersion { get; set; } = string.Empty;
    public string LicenceClassOrEndorsements { get; set; } = string.Empty;
    public string LicenceConditionsNotes { get; set; } = string.Empty;
    public DateTime LicenceIssuedLocal { get; set; } = DateTime.Today;

    public string Line1 { get; set; } = string.Empty;
    public string Line2 { get; set; } = string.Empty;
    public string Suburb { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Region { get; set; } = string.Empty;
    public string Postcode { get; set; } = string.Empty;
    public string Country { get; set; } = string.Empty;

    public string EmergencyFirstName { get; set; } = string.Empty;
    public string EmergencyLastName { get; set; } = string.Empty;
    public string EmergencyRelationship { get; set; } = string.Empty;
    public string EmergencyEmail { get; set; } = string.Empty;
    public string EmergencyPhoneNumber { get; set; } = string.Empty;
    public string EmergencySecondaryPhoneNumber { get; set; } = string.Empty;

    public bool CreateLoginAccount { get; set; }
    public string Password { get; set; } = string.Empty;

    public bool IsEditMode => Guid.TryParse(DriverId, out var id) && id != Guid.Empty;
}
