using System;
using Haulory.Application.Interfaces.Services;
using Microsoft.Maui.Storage;

namespace Haulory.Infrastructure.Services;

public class SessionService : ISessionService
{
    #region Constants

    private const string SessionKey = "haulory_current_account_id";

    #endregion

    #region Properties

    public Guid? CurrentAccountId { get; private set; }

    // True if a user account is currently active
    public bool IsAuthenticated => CurrentAccountId.HasValue;

    #endregion

    #region Session Lifecycle

    public async Task RestoreAsync()
    {
        // Attempt to restore session from secure storage
        var value = await SecureStorage.GetAsync(SessionKey);

        if (Guid.TryParse(value, out var id))
            CurrentAccountId = id;
    }

    public async Task SetAccountAsync(Guid accountId)
    {
        // Persist current account id
        CurrentAccountId = accountId;

        await SecureStorage.SetAsync(
            SessionKey,
            accountId.ToString());
    }

    public async Task ClearAsync()
    {
        // Clear in-memory state
        CurrentAccountId = null;

        // Remove from secure storage
        SecureStorage.Remove(SessionKey);

        await Task.CompletedTask;
    }

    #endregion
}
