using System.Net.Http.Json;
using Haulory.Contracts.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Haulory.Mobile.Diagnostics;

// Handles synchronisation of locally stored crash logs
// from the mobile device to the backend API.
public class CrashSyncService
{
    // Factory for creating EF Core DbContext instances for local crash storage.
    private readonly IDbContextFactory<MobileCrashDbContext> _dbContextFactory;

    // HTTP client used to send crash logs to the API.
    private readonly HttpClient _httpClient;

    // Standard application logger.
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

    // Uploads pending unsynced crash logs to the backend API.
    public async Task SyncPendingAsync(CancellationToken cancellationToken = default)
    {
        // Debug trace to confirm sync execution.
        System.Diagnostics.Debug.WriteLine("CrashSyncService.SyncPendingAsync called");

        // Create local crash database context.
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        // Load a batch of pending crash logs that have not yet been synced.
        var pending = await db.CrashLogs
            .Where(x => !x.IsSynced)
            .OrderBy(x => x.CreatedUtc)
            .Take(25)
            .ToListAsync(cancellationToken);

        System.Diagnostics.Debug.WriteLine($"Pending crash logs: {pending.Count}");

        // Nothing to sync if there are no pending records.
        if (pending.Count == 0)
            return;

        // Build request payload expected by the API.
        var request = new SyncCrashLogsRequest
        {
            Logs = pending.Select(x => x.ToDto()).ToList()
        };

        try
        {
            // Debug output showing target sync endpoint.
            System.Diagnostics.Debug.WriteLine($"Crash sync URL: {_httpClient.BaseAddress}api/crashlogs/bulk");

            // Send crash logs to the backend API.
            var response = await _httpClient.PostAsJsonAsync("api/crashlogs/bulk", request, cancellationToken);

            System.Diagnostics.Debug.WriteLine($"Crash sync response: {(int)response.StatusCode}");

            // Read response body for debugging/troubleshooting.
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            System.Diagnostics.Debug.WriteLine($"Crash sync response body: {body}");

            // Do not mark logs as synced if the API call failed.
            if (!response.IsSuccessStatusCode)
                return;

            // Mark all successfully uploaded logs as synced locally.
            foreach (var item in pending)
                item.IsSynced = true;

            await db.SaveChangesAsync(cancellationToken);

            System.Diagnostics.Debug.WriteLine("Pending crash logs marked as synced");
        }
        catch (Exception ex)
        {
            // Log warning but do not crash the app if sync fails.
            _logger.LogWarning(ex, "Crash sync failed.");
            System.Diagnostics.Debug.WriteLine($"Crash sync failed: {ex.Message}");
        }
    }

    // Retrieves all currently pending unsynced crash logs from local storage.
    public async Task<List<CrashLog>> GetPendingAsync(CancellationToken cancellationToken = default)
    {
        await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

        return await db.CrashLogs
            .Where(x => !x.IsSynced)
            .OrderByDescending(x => x.CreatedUtc)
            .ToListAsync(cancellationToken);
    }
}