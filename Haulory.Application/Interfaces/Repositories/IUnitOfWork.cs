namespace Haulory.Application.Interfaces.Services;

#region Interface: Unit Of Work

public interface IUnitOfWork
{
    #region Transactions

    // Executes the provided action inside a transaction boundary
    // Infrastructure implementation is responsible for:
    // - Beginning transaction
    // - Committing on success
    // - Rolling back on failure
    Task ExecuteInTransactionAsync(Func<Task> action);

    #endregion
}

#endregion
