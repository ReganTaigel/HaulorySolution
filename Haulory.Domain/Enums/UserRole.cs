namespace Haulory.Domain.Enums;

#region Enum: User Role

// Defines account hierarchy within the system
public enum UserRole
{
    // Primary tenant account
    Main = 1,

    // Sub-user under a Main account
    Sub = 2
}

#endregion
