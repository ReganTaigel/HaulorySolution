using Haulory.Api.Extensions;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Haulory.Api.Services;

public sealed class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal User =>
        _httpContextAccessor.HttpContext?.User
        ?? throw new UnauthorizedAccessException("No authenticated user context is available.");

    public Guid GetAccountUserId() => User.GetAccountUserId();

    public Guid GetOwnerUserId() => User.GetOwnerUserId();

    public string GetRoleName() => User.GetRoleName();

    public string? GetEmailAddress() => User.GetEmailAddress();
}