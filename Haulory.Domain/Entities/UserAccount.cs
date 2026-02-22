using Haulory.Domain.Enums;
using Haulory.Domain.Helpers;

namespace Haulory.Domain.Entities;

#region Entity: User Account

public class UserAccount
{
    #region Identity

    public Guid Id { get; private set; } = Guid.NewGuid();

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    #endregion

    #region Authentication

    // Stored hashed password
    public string PasswordHash { get; private set; } = string.Empty;

    #endregion

    #region Roles / Hierarchy

    // Main or Sub user
    public UserRole Role { get; private set; } = UserRole.Main;

    // If Sub, links to main account
    public Guid? ParentMainUserId { get; private set; }

    #endregion

    #region Profile

    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirthUtc { get; private set; }

    public string? Line1 { get; private set; }
    public string? Line2 { get; private set; }

    public string? Suburb { get; private set; }
    public string? City { get; private set; }
    public string? Region { get; private set; }

    public string? Postcode { get; private set; }
    public string? Country { get; private set; }

    public DateTime? LicenceExpiresOnUtc { get; private set; }

    #endregion

    #region Constructors

    // Required by EF Core
    private UserAccount() { }

    // Creates a MAIN account
    public UserAccount(string firstName, string lastName, string email, string passwordHash)
    {
        FirstName = NameFormatter.ToTitleCase(firstName) ?? string.Empty;
        LastName = NameFormatter.ToTitleCase(lastName) ?? string.Empty;
        Email = CleanEmail(email) ?? string.Empty;

        PasswordHash = passwordHash;

        Role = UserRole.Main;
        ParentMainUserId = null;
    }

    // Factory method for creating a SUB account
    public static UserAccount CreateSubUser(
        Guid parentMainUserId,
        string firstName,
        string lastName,
        string email,
        string passwordHash)
    {
        if (parentMainUserId == Guid.Empty)
            throw new ArgumentException("ParentMainUserId required.");

        var u = new UserAccount(firstName, lastName, email, passwordHash);

        u.Role = UserRole.Sub;
        u.ParentMainUserId = parentMainUserId;

        return u;
    }

    #endregion

    #region Identity Updates

    public void UpdateIdentity(string firstName, string lastName, string email)
    {
        // Keep formatting consistent with constructor
        FirstName = NameFormatter.ToTitleCase(firstName) ?? string.Empty;
        LastName = NameFormatter.ToTitleCase(lastName) ?? string.Empty;
        Email = CleanEmail(email) ?? string.Empty;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
    }

    #endregion

    #region Profile Updates

    public void UpdatePhone(string? phone)
    {
        PhoneNumber = Clean(phone);
    }

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
        string? line1,
        string? line2,
        string? suburb,
        string? city,
        string? region,
        string? postcode,
        string? country)
    {
        Line1 = Clean(line1);
        Line2 = Clean(line2);
        Suburb = Clean(suburb);
        City = Clean(city);
        Region = Clean(region);
        Postcode = Clean(postcode);
        Country = Clean(country);
    }

    #endregion

    #region Helpers

    private static string? Clean(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();

    private static string? CleanEmail(string? email)
    {
        // Email should always be trimmed + lower for consistent login/search behavior
        var cleaned = Clean(email);
        return cleaned?.ToLowerInvariant();
    }

    #endregion
}

#endregion