namespace Haulory.Application.Interfaces.Services;

public interface ISessionService
{
    Guid? CurrentAccountId { get; }
    Guid? CurrentOwnerId { get; }
    bool IsAuthenticated { get; }

    string? JwtToken { get; }

    Task RestoreAsync();

    Task SetAccountAsync(Guid accountId, string jwtToken);
    Task SetAccountAsync(Guid accountId, Guid ownerId, string jwtToken);

    Task ClearAsync();

}