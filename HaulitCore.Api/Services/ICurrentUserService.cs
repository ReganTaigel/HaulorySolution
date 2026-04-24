namespace HaulitCore.Api.Services;

// Defines a contract for accessing information about the currently authenticated user.
// Allows application and domain services to retrieve user identity data without depending on HttpContext.
public interface ICurrentUserService
{
    // Returns the account user ID (the authenticated user making the request).
    Guid GetAccountUserId();

    // Returns the owner user ID (used for tenant or business-level scoping).
    Guid GetOwnerUserId();

    // Returns the role name assigned to the current user.
    string GetRoleName();

    // Returns the user's email address, if available.
    string? GetEmailAddress();
}