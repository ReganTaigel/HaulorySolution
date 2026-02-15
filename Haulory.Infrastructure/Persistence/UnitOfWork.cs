using Haulory.Application.Interfaces.Services;
using Haulory.Infrastructure.Persistence;

namespace Haulory.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly HauloryDbContext _db;

    public UnitOfWork(HauloryDbContext db)
    {
        _db = db;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> action)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();
        await action();
        await tx.CommitAsync();
    }
}
