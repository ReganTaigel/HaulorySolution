namespace Haulory.Api.Services;

public interface ICurrentUserService
{
    Guid GetAccountUserId();
    Guid GetOwnerUserId();
    string GetRoleName();
    string? GetEmailAddress();
}