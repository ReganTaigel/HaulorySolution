using HaulitCore.Application.Interfaces.Services;
using Microsoft.Maui.Storage;

namespace HaulitCore.Application.Services;

public sealed class SessionService : ISessionService
{
    private const string AccountIdKey = "session_account_id";
    private const string OwnerIdKey = "session_owner_id";
    private const string JwtTokenKey = "session_jwt_token";

    public Guid? CurrentAccountId { get; private set; }
    public Guid? CurrentOwnerId { get; private set; }
    public string? JwtToken { get; private set; }

    public bool IsAuthenticated =>
        CurrentAccountId.HasValue &&
        CurrentOwnerId.HasValue &&
        !string.IsNullOrWhiteSpace(JwtToken);

    public Task RestoreAsync()
    {
        var accountRaw = Preferences.Default.Get(AccountIdKey, string.Empty);
        var ownerRaw = Preferences.Default.Get(OwnerIdKey, string.Empty);
        var tokenRaw = Preferences.Default.Get(JwtTokenKey, string.Empty);

        CurrentAccountId = Guid.TryParse(accountRaw, out var accountId) ? accountId : null;
        CurrentOwnerId = Guid.TryParse(ownerRaw, out var ownerId) ? ownerId : null;
        JwtToken = string.IsNullOrWhiteSpace(tokenRaw) ? null : tokenRaw;

        return Task.CompletedTask;
    }

    public Task SetAccountAsync(Guid accountId, string jwtToken)
    {
        CurrentAccountId = accountId;
        CurrentOwnerId = accountId;
        JwtToken = jwtToken;

        Preferences.Default.Set(AccountIdKey, accountId.ToString());
        Preferences.Default.Set(OwnerIdKey, accountId.ToString());
        Preferences.Default.Set(JwtTokenKey, jwtToken);

        return Task.CompletedTask;
    }

    public Task SetAccountAsync(Guid accountId, Guid ownerId, string jwtToken)
    {
        CurrentAccountId = accountId;
        CurrentOwnerId = ownerId;
        JwtToken = jwtToken;

        Preferences.Default.Set(AccountIdKey, accountId.ToString());
        Preferences.Default.Set(OwnerIdKey, ownerId.ToString());
        Preferences.Default.Set(JwtTokenKey, jwtToken);

        return Task.CompletedTask;
    }

    public Task ClearAsync()
    {
        CurrentAccountId = null;
        CurrentOwnerId = null;
        JwtToken = null;

        Preferences.Default.Remove(AccountIdKey);
        Preferences.Default.Remove(OwnerIdKey);
        Preferences.Default.Remove(JwtTokenKey);

        return Task.CompletedTask;
    }
}