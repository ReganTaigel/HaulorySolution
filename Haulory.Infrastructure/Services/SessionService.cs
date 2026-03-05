using System;
using Haulory.Application.Interfaces.Services;
using Microsoft.Maui.Storage;

namespace Haulory.Infrastructure.Services;

public class SessionService : ISessionService
{
    #region Constants

    private const string AccountKey = "haulory_current_account_id";
    private const string OwnerKey = "haulory_current_owner_id";

    #endregion

    #region Properties

    public Guid? CurrentAccountId { get; private set; }

    // ✅ NEW: Tenant boundary
    public Guid? CurrentOwnerId { get; private set; }

    // True if a user account is currently active
    public bool IsAuthenticated => CurrentAccountId.HasValue;

    #endregion

    #region Session Lifecycle

    public async Task RestoreAsync()
    {
        var accountValue = await SecureStorage.GetAsync(AccountKey);
        var ownerValue = await SecureStorage.GetAsync(OwnerKey);

        if (Guid.TryParse(accountValue, out var accountId))
            CurrentAccountId = accountId;

        if (Guid.TryParse(ownerValue, out var ownerId))
            CurrentOwnerId = ownerId;

        // ✅ Back-compat: if older installs only have AccountKey, treat it as owner too
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

    #endregion
} 