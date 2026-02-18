namespace Haulory.Application.Features.Users;

#region Command: Register User

public record RegisterUserCommand(

#region Basic Information

    // User first name
    string FirstName,

    // User last name
    string LastName,

    // User email (will be normalized in handler)
    string Email,

#endregion

#region Credentials

    // Plain-text password (must be hashed in handler)
    string Password

#endregion
);

#endregion
