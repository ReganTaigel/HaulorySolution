namespace Haulory.Application.Interfaces.Services;

public interface ISessionService
{
    Guid? CurrentAccountId { get; }
    bool IsAuthenticated { get; }

    Task RestoreAsync();
    Task SetAccountAsync(Guid accountId);
    Task ClearAsync();
}
