using System.Net.Http.Json;
using Haulory.Contracts.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Haulory.Mobile.Diagnostics;

public class CrashSyncService
{
    private readonly IDbContextFactory<MobileCrashDbContext> _dbContextFactory;
    private readonly HttpClient _httpClient;
    private readonly ILogger<CrashSyncService> _logger;

    public CrashSyncService(
        IDbContextFactory<MobileCrashDbContext> dbContextFactory,
        HttpClient httpClient,
        ILogger<CrashSyncService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task SyncPendingAsync(CancellationToken cancellationToken = default)
    {
        System.Diagnostics.Debug.WriteLine("CrashSyncService.SyncPendingAsync called");

        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        var pending = await db.CrashLogs
            .Where(x => !x.IsSynced)
            .OrderBy(x => x.CreatedUtc)
            .Take(25)
            .ToListAsync(cancellationToken);

        System.Diagnostics.Debug.WriteLine($"Pending crash logs: {pending.Count}");

        if (pending.Count == 0)
            return;

        var request = new SyncCrashLogsRequest
        {
            Logs = pending.Select(x => x.ToDto()).ToList()
        };

        try
        {
            System.Diagnostics.Debug.WriteLine($"Crash sync URL: {_httpClient.BaseAddress}api/crashlogs/bulk");

            var response = await _httpClient.PostAsJsonAsync("api/crashlogs/bulk", request, cancellationToken);

            System.Diagnostics.Debug.WriteLine($"Crash sync response: {(int)response.StatusCode}");

            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"Crash sync response body: {body}");

            if (!response.IsSuccessStatusCode)
                return;

            foreach (var item in pending)
                item.IsSynced = true;

            await db.SaveChangesAsync(cancellationToken);

            System.Diagnostics.Debug.WriteLine("Pending crash logs marked as synced");
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Crash sync failed.");
            System.Diagnostics.Debug.WriteLine($"Crash sync failed: {ex.Message}");
        }
    }

    public async Task<List<CrashLog>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.CrashLogs
            .Where(x => !x.IsSynced)
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);
    }

}