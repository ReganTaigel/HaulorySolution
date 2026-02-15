using Haulory.Domain.Enums;

namespace Haulory.Domain.Entities;

public class UserAccount
{
    public Guid Id { get; private set; } = Guid.NewGuid();

    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;

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

    public void UpdateIdentity(string firstName, string lastName, string email)
    {
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
        Email = email.Trim().ToLowerInvariant();
    }

    public void SetPasswordHash(string passwordHash) => PasswordHash = passwordHash;
}
