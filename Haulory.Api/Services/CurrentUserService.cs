using Haulory.Api.Extensions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Haulory.Api.Services;

// Provides access to the currently authenticated user's identity and claims.
// Acts as an abstraction over HttpContext to make user access testable and reusable.
public sealed class CurrentUserService : ICurrentUserService
{
    // Used to access the current HTTP context.
    private readonly IHttpContextAccessor _httpContextAccessor;

    // Constructor injection of IHttpContextAccessor.
    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // Retrieves the ClaimsPrincipal for the current request.
    // Throws if no authenticated user context is available.
    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("No authenticated user context is available.");

    // Returns the account user ID (typically the logged-in user).
    public Guid GetAccountUserId() => User.GetAccountUserId();

    // Returns the owner user ID (used for tenant/business scoping).
    public Guid GetOwnerUserId() => User.GetOwnerUserId();

    // Returns the role name assigned to the user.
    public string GetRoleName() => User.GetRoleName();

    // Returns the email address associated with the user, if available.
    public string? GetEmailAddress() => User.GetEmailAddress();
}