namespace HaulitCore.Contracts.Auth;

public sealed class RegisterRequest
{
    // User
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    // Contact
    public string? PhoneNumber { get; set; }

    public DateTime? DateOfBirthUtc { get; set; }

    // Personal Address
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? Suburb { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? Postcode { get; set; }
    public string? Country { get; set; }

    // Business
    public string? BusinessName { get; set; }
    public string? BusinessEmail { get; set; }
    public string? BusinessPhone { get; set; }

    public string? BusinessAddress1 { get; set; }
    public string? BusinessSuburb { get; set; }
    public string? BusinessCity { get; set; }
    public string? BusinessRegion { get; set; }
    public string? BusinessPostcode { get; set; }
    public string? BusinessCountry { get; set; }

    // Supplier info
    public string? SupplierNzbn { get; set; }
    public string? SupplierGstNumber { get; set; }
    public string? BankAccountNumber {  get; set; }
}