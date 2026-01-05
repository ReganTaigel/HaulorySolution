using Haulory.Domain.Entities;

namespace Haulory.Application.Interfaces.Services;

public interface ISessionService
{
    User? CurrentUser { get; }
    bool IsAuthenticated { get; }

    Task RestoreAsync();
    Task SetUserAsync(User user);
    Task ClearAsync();
}
