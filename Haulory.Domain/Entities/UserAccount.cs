using Haulory.Domain.Enums;

namespace Domain.Entities;

public class UserAccount
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;

    public UserRole Role { get; set; } = UserRole.Main;

    // If this is a Sub user, link back to its Main user
    public Guid? ParentMainUserId { get; set; }
}
