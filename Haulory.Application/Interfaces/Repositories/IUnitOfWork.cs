namespace Haulory.Application.Interfaces.Services;

public interface IUnitOfWork
{
    Task ExecuteInTransactionAsync(Func<Task> action);
}
