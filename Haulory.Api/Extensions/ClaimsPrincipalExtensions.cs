using System.Security.Claims;

namespace Haulory.Api.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid GetAccountUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var value =
            user.FindFirstValue("account_id") ??
            user.FindFirstValue("accountId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("sub") ??
            user.FindFirstValue("userId");

        if (!Guid.TryParse(value, out var accountUserId))
            throw new UnauthorizedAccessException("Authenticated account id is missing or invalid.");

        return accountUserId;
    }

    public static Guid GetOwnerUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var value =
            user.FindFirstValue("owner_id") ??
            user.FindFirstValue("ownerId") ??
            user.FindFirstValue("account_id") ??
            user.FindFirstValue("accountId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("sub") ??
            user.FindFirstValue("userId");

        if (!Guid.TryParse(value, out var ownerUserId))
            throw new UnauthorizedAccessException("Authenticated owner id is missing or invalid.");

        return ownerUserId;
    }
    public static Guid GetUserId(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        var value =
            user.FindFirstValue("user_id") ??
            user.FindFirstValue("userId") ??
            user.FindFirstValue(ClaimTypes.NameIdentifier) ??
            user.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
            throw new UnauthorizedAccessException("Authenticated user id is missing or invalid.");

        return userId;
    }
    public static string GetRoleName(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return user.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }

    public static string? GetEmailAddress(this ClaimsPrincipal user)
    {
        ArgumentNullException.ThrowIfNull(user);

        return user.FindFirstValue(ClaimTypes.Email)
            ?? user.FindFirstValue("email")
            ?? user.FindFirstValue("emails");
    }

    public static bool IsInRoleName(this ClaimsPrincipal user, string roleName)
    {
        ArgumentNullException.ThrowIfNull(user);

        var role = user.GetRoleName();
        return string.Equals(role, roleName, StringComparison.OrdinalIgnoreCase);
    }

}