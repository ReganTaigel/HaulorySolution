using System.Security.Claims;

namespace HaulitCore.Api.Extensions;

// Provides extension methods for extracting strongly-typed values from ClaimsPrincipal.
// Centralises claim access logic and supports multiple claim naming conventions.
public static class ClaimsPrincipalExtensions
{
    // Retrieves the account user ID (primary authenticated user).
    public static Guid GetAccountUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Attempt to resolve account ID from multiple possible claim names.
        var value =
            user.FindFirstValue("account_id") ??
            user.FindFirstValue("accountId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("sub") ??
            user.FindFirstValue("userId");

        // Ensure the value is a valid GUID.
        if (!Guid.TryParse(value, out var accountUserId))
            throw new UnauthorizedAccessException("Authenticated account id is missing or invalid.");

        return accountUserId;
    }

    // Retrieves the owner user ID (used for tenant/business scoping).
    public static Guid GetOwnerUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Attempt to resolve owner ID from multiple possible claim names.
        // Falls back to account ID if owner-specific claim is not present.
        var value =
            user.FindFirstValue("owner_id") ??
            user.FindFirstValue("ownerId") ??
            user.FindFirstValue("account_id") ??
            user.FindFirstValue("accountId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("sub") ??
            user.FindFirstValue("userId");

        // Ensure the value is a valid GUID.
        if (!Guid.TryParse(value, out var ownerUserId))
            throw new UnauthorizedAccessException("Authenticated owner id is missing or invalid.");

        return ownerUserId;
    }

    // Retrieves a generic user ID (used in contexts such as driver actions).
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Attempt to resolve user ID from multiple possible claim names.
        var value =
            user.FindFirstValue("user_id") ??
            user.FindFirstValue("userId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("sub");

        // Ensure the value is a valid GUID.
        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("Authenticated user id is missing or invalid.");

        return userId;
    }

    // Retrieves the role name assigned to the user.
    public static string GetRoleName(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Return role claim or empty string if not present.
        return user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }

    // Retrieves the user's email address, if available.
    public static string? GetEmailAddress(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        // Support multiple possible email claim formats.
        return user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue("email")
            ?? user.FindFirstValue("emails");
    }

    // Checks whether the user has a specific role (case-insensitive).
    public static bool IsInRoleName(this ClaimsPrincipal user, string roleName)
    {
        ArgumentNullException.ThrowIfNull(user);

        var role = user.GetRoleName();

        return string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase);
    }
}