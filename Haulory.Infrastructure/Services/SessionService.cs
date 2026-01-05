using System.Text.Json;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;

namespace Haulory.Infrastructure.Services;

public class SessionService : ISessionService
{
    private const string SessionKey = "haulory_current_user";

    public User? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser != null;

    public async Task RestoreAsync()
    {
        var json = await SecureStorage.GetAsync(SessionKey);

        if (string.IsNullOrWhiteSpace(json))
            return;

        CurrentUser = JsonSerializer.Deserialize<User>(json);
    }

    public async Task SetUserAsync(User user)
    {
        CurrentUser = user;

        var json = JsonSerializer.Serialize(user);
        await SecureStorage.SetAsync(SessionKey, json);
    }

    public async Task ClearAsync()
    {
        CurrentUser = null;
        SecureStorage.Remove(SessionKey);
        await Task.CompletedTask;
    }
}
