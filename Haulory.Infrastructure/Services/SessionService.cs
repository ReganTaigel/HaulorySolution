using Haulory.Application.Interfaces.Services;

public class SessionService : ISessionService
{
    private const string SessionKey = "haulory_current_account_id";

    public Guid? CurrentAccountId { get; private set; }
    public bool IsAuthenticated => CurrentAccountId.HasValue;

    public async Task RestoreAsync()
    {
        var value = await SecureStorage.GetAsync(SessionKey);
        if (Guid.TryParse(value, out var id))
            CurrentAccountId = id;
    }

    public async Task SetAccountAsync(Guid accountId)
    {
        CurrentAccountId = accountId;
        await SecureStorage.SetAsync(SessionKey, accountId.ToString());
    }

    public async Task ClearAsync()
    {
        CurrentAccountId = null;
        SecureStorage.Remove(SessionKey);
        await Task.CompletedTask;
    }
}
