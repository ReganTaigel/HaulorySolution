namespace Haulory.Application.Features.Drivers;

#region Command: Create Driver From Existing User

// Command used to create a Driver aggregate
// from an already existing authenticated User. 
// This differs from CreateDriverCommand:
// - UserId is required (driver has login account)
// - OwnerUserId is inferred from the User context (handled in the handler)
public record CreateDriverFromUserCommand(

#region Identity

    // The existing authenticated user's Id.
    Guid UserId,

#endregion

#region Basic Profile

    // Driver first name (will be normalized in handler).
    string FirstName,

    // Driver last name (will be normalized in handler).
    string LastName,

    // Driver email address.
    string Email

#endregion
);

#endregion
