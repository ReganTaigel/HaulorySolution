namespace Haulory.Contracts.Drivers;

public sealed class EmergencyContactDto
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Relationship { get; set; }
    public string? PhoneNumber { get; set; }
    public string? SecondaryPhoneNumber { get; set; }
    public string? Email { get; set; }
}