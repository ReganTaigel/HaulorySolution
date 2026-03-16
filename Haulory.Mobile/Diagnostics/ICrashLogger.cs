namespace Haulory.Mobile.Diagnostics;

public interface ICrashLogger
{
    Task LogAsync(
        Exception exception,
        string source,
        bool isHandled,
        string severity = "Error",
        string? pageName = null,
        string? metadataJson = null,
        CancellationToken cancellationToken = default);

    Task LogMessageAsync(
        string message,
        string source,
        bool isHandled,
        string severity = "Error",
        string? pageName = null,
        string? metadataJson = null,
        CancellationToken cancellationToken = default);

    void TryLogCriticalImmediately(
        Exception exception,
        string source,
        bool isHandled,
        string severity = "Critical",
        string? pageName = null,
        string? metadataJson = null);

    void TryLogMessageCriticalImmediately(
        string message,
        string source,
        bool isHandled,
        string severity = "Critical",
        string? pageName = null,
        string? metadataJson = null);
}