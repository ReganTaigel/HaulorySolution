namespace HaulitCore.Mobile.Diagnostics;

// Defines the contract for crash logging within the mobile application.
// Supports both standard async logging and immediate fallback logging for critical failures.
public interface ICrashLogger
{
    // Logs a full exception asynchronously using the normal logging pipeline (EF + DB).
    Task LogAsync(
        Exception exception,
        string source,
        bool isHandled,
        string severity = "Error",
        string? pageName = null,
        string? metadataJson = null,
        CancellationToken cancellationToken = default);

    // Logs a non-exception message asynchronously using the normal logging pipeline.
    Task LogMessageAsync(
        string message,
        string source,
        bool isHandled,
        string severity = "Error",
        string? pageName = null,
        string? metadataJson = null,
        CancellationToken cancellationToken = default);

    // Logs a critical exception immediately using a fallback mechanism
    // (bypasses EF/DI to ensure logging works during severe failures).
    void TryLogCriticalImmediately(
        Exception exception,
        string source,
        bool isHandled,
        string severity = "Critical",
        string? pageName = null,
        string? metadataJson = null);

    // Logs a critical message immediately using the fallback mechanism.
    void TryLogMessageCriticalImmediately(
        string message,
        string source,
        bool isHandled,
        string severity = "Critical",
        string? pageName = null,
        string? metadataJson = null);
}