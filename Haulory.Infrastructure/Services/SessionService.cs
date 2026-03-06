using System;
using System.Threading.Tasks;
using Haulory.Application.Interfaces.Services;
using Microsoft.Maui.Storage;

namespace Haulory.Infrastructure.Services;

public class SessionService : ISessionService
{
    private const string AccountKey = "haulory_current_account_id";
    private const string OwnerKey = "haulory_current_owner_id";

    public Guid? CurrentAccountId { get; private set; }
    public Guid? CurrentOwnerId { get; private set; }

    public bool IsAuthenticated => CurrentAccountId.HasValue;

    public async Task RestoreAsync()
    {
        var accountValue = await SecureStorage.GetAsync(AccountKey);
        var ownerValue = await SecureStorage.GetAsync(OwnerKey);

        if (Guid.TryParse(accountValue, out var accountId))
            CurrentAccountId = accountId;

        if (Guid.TryParse(ownerValue, out var ownerId))
            CurrentOwnerId = ownerId;

        // Back-compat: if older installs only have AccountKey, treat it as owner too
        if (CurrentAccountId.HasValue && !CurrentOwnerId.HasValue)
            CurrentOwnerId = CurrentAccountId;
    }

    public async Task SetAccountAsync(Guid accountId)
    {
        // Main account login: owner == account
        CurrentAccountId = accountId;
        CurrentOwnerId = accountId;

        await SecureStorage.SetAsync(AccountKey, accountId.ToString());
        await SecureStorage.SetAsync(OwnerKey, accountId.ToString());
    }

    public async Task SetAccountAsync(Guid accountId, Guid ownerId)
    {
        // Sub account login: owner != account
        CurrentAccountId = accountId;
        CurrentOwnerId = ownerId;

        await SecureStorage.SetAsync(AccountKey, accountId.ToString());
        await SecureStorage.SetAsync(OwnerKey, ownerId.ToString());
    }

    public async Task ClearAsync()
    {
        CurrentAccountId = null;
        CurrentOwnerId = null;

        SecureStorage.Remove(AccountKey);
        SecureStorage.Remove(OwnerKey);

        await Task.CompletedTask;
    }
}