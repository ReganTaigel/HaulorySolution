using HaulitCore.Domain.Enums;
using HaulitCore.Domain.Helpers;

namespace HaulitCore.Domain.Entities;

public class UserAccount
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    // Identity / Login
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;

    // Login identity (keep your current name to avoid refactors)
    public string Email { get; private set; } = string.Empty;

    // Authentication
    public string PasswordHash { get; private set; } = string.Empty;

    // Roles / Hierarchy
    public UserRole Role { get; private set; } = UserRole.Main;
    public Guid? ParentMainUserId { get; private set; }

    // Helpers (tenant resolution)
    public bool IsMainAccount => ParentMainUserId == null;
    public Guid OwnerUserId => ParentMainUserId ?? Id;

    // Personal profile (existing)
    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirthUtc { get; private set; }

    public string? Line1 { get; private set; }
    public string? Suburb { get; private set; }
    public string? City { get; private set; }
    public string? Region { get; private set; }
    public string? Postcode { get; private set; }
    public string? Country { get; private set; }

    public DateTime? LicenceExpiresOnUtc { get; private set; }

    //  Business profile (Supplier) for PDFs
    public string BusinessName { get; private set; } = string.Empty;
    public string? BusinessEmail { get; private set; }
    public string? BusinessPhone { get; private set; }

    public string? BusinessAddress1 { get; private set; }
    public string? BusinessSuburb { get; private set; }
    public string? BusinessCity { get; private set; }
    public string? BusinessRegion { get; private set; }
    public string? BusinessPostcode { get; private set; }
    public string? BusinessCountry { get; private set; }
    
    public string? SupplierGstNumber { get; private set; }
    public string? SupplierNzbn { get; private set; }
    public string? BankAccountNumber { get; private set; }

    // EF
    private UserAccount() { }

    // MAIN
    public UserAccount(string firstName, string lastName, string email, string passwordHash)
    {
        FirstName = NameFormatter.ToTitleCase(firstName) ?? string.Empty;
        LastName = NameFormatter.ToTitleCase(lastName) ?? string.Empty;
        Email = CleanEmail(email) ?? string.Empty;

        PasswordHash = passwordHash;

        Role = UserRole.Main;
        ParentMainUserId = null;
    }

    public static UserAccount CreateSubUser(
        Guid parentMainUserId,
        string firstName,
        string lastName,
        string email,
        string passwordHash)
    {
        if (parentMainUserId == Guid.Empty)
            throw new ArgumentException("ParentMainUserId required.");

        var u = new UserAccount(firstName, lastName, email, passwordHash)
        {
            Role = UserRole.Sub,
            ParentMainUserId = parentMainUserId
        };

        return u;
    }

    public void UpdateIdentity(string firstName, string lastName, string email)
    {
        FirstName = NameFormatter.ToTitleCase(firstName) ?? string.Empty;
        LastName = NameFormatter.ToTitleCase(lastName) ?? string.Empty;
        Email = CleanEmail(email) ?? string.Empty;
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;

    public void UpdatePhone(string? phone) => PhoneNumber = Clean(phone);

    public void UpdateDateOfBirthUtc(DateTime? dobUtc)
    {
        DateOfBirthUtc = dobUtc.HasValue
            ? DateTime.SpecifyKind(dobUtc.Value, DateTimeKind.Utc)
            : null;
    }

    public void UpdateLicenceExpiryUtc(DateTime? expiresUtc)
    {
        LicenceExpiresOnUtc = expiresUtc.HasValue
            ? DateTime.SpecifyKind(expiresUtc.Value, DateTimeKind.Utc)
            : null;
    }

    public void UpdateAddress(
        string? line1,  string? suburb, string? city,
        string? region, string? postcode, string? country)
    {
        Line1 = Clean(line1);
        Suburb = Clean(suburb);
        City = Clean(city);
        Region = Clean(region);
        Postcode = Clean(postcode);
        Country = Clean(country);
    }

    // Business profile updates
    public void UpdateBusinessIdentity(
        string businessName,
        string? businessEmail,
        string? gstNumber,
        string? nzbn,
        string? bankAccountNumber)
    {
        BusinessName = string.IsNullOrWhiteSpace(businessName)
            ? string.Empty
            : businessName.Trim();

        BusinessEmail = string.IsNullOrWhiteSpace(businessEmail)
            ? null
            : businessEmail.Trim().ToLowerInvariant();

        SupplierGstNumber = string.IsNullOrWhiteSpace(gstNumber) ? null : gstNumber.Trim();
        SupplierNzbn = string.IsNullOrWhiteSpace(nzbn) ? null : nzbn.Trim();

        BankAccountNumber = string.IsNullOrWhiteSpace(bankAccountNumber)
            ? null
            : bankAccountNumber.Replace(" ", "").Trim();
    }

    public void UpdateBusinessContact(string? businessPhone)
    {
        BusinessPhone = Clean(businessPhone);
    }

    public void UpdateBusinessAddress(
        string? line1, string? suburb, string? city,
        string? region, string? postcode, string? country)
    {
        BusinessAddress1 = Clean(line1);
        BusinessSuburb = Clean(suburb);
        BusinessCity = Clean(city);
        BusinessRegion = Clean(region);
        BusinessPostcode = Clean(postcode);
        BusinessCountry = Clean(country);
    }

    private static string? Clean(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string? CleanEmail(string? email) =>
        Clean(email)?.ToLowerInvariant();
}