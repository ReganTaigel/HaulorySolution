namespace Haulory.Application.Features.Users;

#region Command: Login User

// Command used to authenticate a user. 
// This contains the raw credentials submitted by the client.
// Validation and authentication logic are handled in the corresponding handler.
public record LoginUserCommand(

#region Credentials

    // User email address.
    // Should be normalized (trimmed + lowercase) in the handler.
    string Email,

    // Plain-text password supplied by the user.
    // The handler is responsible for hashing/comparing securely.
    string Password

#endregion
);

#endregion
