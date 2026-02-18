namespace Haulory.Application.Interfaces.Services;

#region Interface: Session Service

public interface ISessionService
{
    #region State

    // Currently authenticated account Id (null if not logged in)
    Guid? CurrentAccountId { get; }

    // Indicates whether a user is authenticated
    bool IsAuthenticated { get; }

    #endregion

    #region Lifecycle

    // Restores session from persistent storage (e.g., secure storage)
    Task RestoreAsync();

    // Sets the active account session
    Task SetAccountAsync(Guid accountId);

    // Clears the current session (logout)
    Task ClearAsync();

    #endregion
}

#endregion
