using System;
using HaulitCore.Application.Interfaces.Services;

namespace HaulitCore.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    #region Dependencies

    private readonly HaulitCoreDbContext _db;

    #endregion

    #region Constructor

    public UnitOfWork(HaulitCoreDbContext db)
    {
        _db = db;
    }

    #endregion

    #region Transaction Execution

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        // Begin database transaction
        await using var tx = await _db.Database.BeginTransactionAsync();

        // Execute caller logic
        await action();

        // Commit transaction if no exception occurred
        await tx.CommitAsync();
    }

    #endregion
}
