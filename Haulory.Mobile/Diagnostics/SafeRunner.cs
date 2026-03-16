namespace Haulory.Mobile.Diagnostics;

public static class SafeRunner
{
    public static async Task RunAsync(
        Func<Task> action,
        ICrashLogger crashLogger,
        string source,
        string? pageName = null,
        string? metadataJson = null,
        Func<Exception, Task>? onError = null,
        bool rethrow = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await action();
        }
        catch (Exception ex)
        {
            await crashLogger.LogAsync(
                ex,
                source,
                true,
                "Error",
                pageName,
                metadataJson,
                cancellationToken);

            if (onError is not null)
                await onError(ex);

            if (rethrow)
                throw;
        }
    }

    public static async Task<T?> RunAsync<T>(
        Func<Task<T>> action,
        ICrashLogger crashLogger,
        string source,
        string? pageName = null,
        string? metadataJson = null,
        Func<Exception, Task>? onError = null,
        bool rethrow = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await action();
        }
        catch (Exception ex)
        {
            await crashLogger.LogAsync(
                ex,
                source,
                true,
                "Error",
                pageName,
                metadataJson,
                cancellationToken);

            if (onError is not null)
                await onError(ex);

            if (rethrow)
                throw;

            return default;
        }
    }
}