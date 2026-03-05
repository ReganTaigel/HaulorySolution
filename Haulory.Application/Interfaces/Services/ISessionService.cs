namespace Haulory.Application.Interfaces.Services;

#region Interface: Session Service

public interface ISessionService
{
    #region State

    // Logged in user (driver / dispatcher / main)
    Guid? CurrentAccountId { get; }

    // Tenant boundary (main account)
    Guid? CurrentOwnerId { get; }

    // Indicates whether a user is authenticated
    bool IsAuthenticated { get; }

    #endregion

    #region Lifecycle

    Task RestoreAsync();

    // MAIN account login
    Task SetAccountAsync(Guid accountId);

    // SUB user login
    Task SetAccountAsync(Guid accountId, Guid ownerId);

    Task ClearAsync();

    #endregion
}

#endregion