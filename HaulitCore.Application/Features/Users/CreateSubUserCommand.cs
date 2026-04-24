namespace HaulitCore.Application.Features.Users;

// Represents the application command used to create a sub-user account.
// A sub-user belongs to an existing owner/main user (tenant root).
public record CreateSubUserCommand(
    // The authenticated account making the request.
    // Used to identify who initiated the action.
    Guid RequestorAccountId,

    // The main owner account for the tenant/business.
    // All sub-users created through this command belong to this owner.
    Guid OwnerMainUserId,

    // Core identity details for the new sub-user.
    string FirstName,
    string LastName,
    string Email,

    // Password for the new sub-user login account.
    string Password
);