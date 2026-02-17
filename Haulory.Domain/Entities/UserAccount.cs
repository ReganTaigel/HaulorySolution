using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class UserAccount
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

    // NEW: Contact + profile
    public string? PhoneNumber { get; private set; }
    public DateTime? DateOfBirthUtc { get; private set; }

    // NEW: Address (stored directly on UserAccount)
    public string? Line1 { get; private set; }
    public string? Line2 { get; private set; }

    public string? Suburb { get; private set; }
    public string? City { get; private set; }
    public string? Region { get; private set; }

    public string? Postcode { get; private set; }
    public string? Country { get; private set; }

    // NEW: Licence expiry (optional)
    public DateTime? LicenceExpiresOnUtc { get; private set; }

    // Auth
    public string PasswordHash { get; private set; } = string.Empty;

    // Roles / hierarchy
    public UserRole Role { get; private set; } = UserRole.Main;
    public Guid? ParentMainUserId { get; private set; }

    public UserAccount() { } // EF

    public UserAccount(string firstName, string lastName, string email, string passwordHash)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = UserRole.Main;
        ParentMainUserId = null;
    }

    public static UserAccount CreateSubUser(Guid parentMainUserId, string firstName, string lastName, string email, string passwordHash)
    {
        if (parentMainUserId == Guid.Empty) throw new ArgumentException("ParentMainUserId required.");

        var u = new UserAccount(firstName, lastName, email, passwordHash);
        u.Role = UserRole.Sub;
        u.ParentMainUserId = parentMainUserId;
        return u;
    }

    // -------------------------
    // Identity
    // -------------------------
    public void UpdateIdentity(string firstName, string lastName, string email)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;

    // -------------------------
    // NEW: Profile updates
    // -------------------------
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

    private static string? Clean(string? s) =>
        string.IsNullOrWhiteSpace(s) ? null : s.Trim();
}
