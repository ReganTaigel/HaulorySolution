using HaulitCore.Application.Interfaces.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HaulitCore.Mobile.Diagnostics;

// Provides crash logging functionality for the mobile application.
// Logs errors to a local SQLite database and standard logging system.
public class CrashLogger : ICrashLogger
{
    // Factory for creating EF Core DbContext instances.
    private readonly IDbContextFactory<MobileCrashDbContext> _dbContextFactory;

    // Standard application logger.
    private readonly ILogger<CrashLogger> _logger;

    // Provides access to current session/user context.
    private readonly ISessionService _sessionService;

    // Path to the local SQLite database used for crash logging.
    private readonly string _dbPath;

    public CrashLogger(
        IDbContextFactory<MobileCrashDbContext> dbContextFactory,
        ILogger<CrashLogger> logger,
        ISessionService sessionService)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _sessionService = sessionService;
        _dbPath = Path.Combine(FileSystem.AppDataDirectory, "HaulitCore-crashlogs.db");
    }

    // Logs a full exception asynchronously using EF Core.
    public async Task LogAsync(
        Exception exception,
        string source,
        bool isHandled,
        string severity = "Error",
        string? pageName = null,
        string? metadataJson = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Log to standard logger.
            _logger.LogError(exception, "Crash logged from {Source}", source);

            // Create database context.
            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            // Build crash log entry.
            var entry = BuildEntry(
                message: exception.Message,
                source: source,
                isHandled: isHandled,
                severity: severity,
                pageName: pageName,
                metadataJson: metadataJson,
                stackTrace: exception.ToString(),
                innerException: exception.InnerException?.ToString(),
                exceptionType: exception.GetType().FullName);

            // Save to database.
            db.CrashLogs.Add(entry);
            await db.SaveChangesAsync(cancellationToken);

            // Debug output.
            System.Diagnostics.Debug.WriteLine(
                $"[CrashLogger] Crash log saved locally: Id={entry.Id}, Source={entry.Source}, Message={entry.Message}");
        }
        catch (Exception loggingException)
        {
            // Fallback logging if crash logging itself fails.
            _logger.LogCritical(loggingException, "Failed to write crash log.");
            System.Diagnostics.Debug.WriteLine(
                $"[CrashLogger] Failed to save crash log locally: {loggingException}");
        }
    }

    // Logs a simple message (non-exception) asynchronously.
    public async Task LogMessageAsync(
        string message,
        string source,
        bool isHandled,
        string severity = "Error",
        string? pageName = null,
        string? metadataJson = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogError("Crash message logged from {Source}: {Message}", source, message);

            await using var db = await _dbContextFactory.CreateDbContextAsync(cancellationToken);

            var entry = BuildEntry(
                message: message,
                source: source,
                isHandled: isHandled,
                severity: severity,
                pageName: pageName,
                metadataJson: metadataJson,
                stackTrace: null,
                innerException: null,
                exceptionType: null);

            db.CrashLogs.Add(entry);
            await db.SaveChangesAsync(cancellationToken);

            System.Diagnostics.Debug.WriteLine(
                $"[CrashLogger] Crash message saved locally: Id={entry.Id}, Source={entry.Source}, Message={entry.Message}");
        }
        catch (Exception loggingException)
        {
            _logger.LogCritical(loggingException, "Failed to write crash message.");
            System.Diagnostics.Debug.WriteLine(
                $"[CrashLogger] Failed to save crash message locally: {loggingException}");
        }
    }

    // Logs critical crashes immediately using direct SQLite access (bypasses EF).
    public void TryLogCriticalImmediately(
        Exception exception,
        string source,
        bool isHandled,
        string severity = "Critical",
        string? pageName = null,
        string? metadataJson = null)
    {
        try
        {
            _logger.LogCritical(exception, "Critical crash logged immediately from {Source}", source);

            var entry = BuildEntry(
                message: exception.Message,
                source: source,
                isHandled: isHandled,
                severity: severity,
                pageName: pageName,
                metadataJson: metadataJson,
                stackTrace: exception.ToString(),
                innerException: exception.InnerException?.ToString(),
                exceptionType: exception.GetType().FullName);

            // Write directly to SQLite to avoid EF/DI failures.
            WriteDirectToSqlite(entry);

            System.Diagnostics.Debug.WriteLine(
                $"[CrashLogger] Critical crash saved immediately: Id={entry.Id}, Source={entry.Source}, Message={entry.Message}");
        }
        catch (Exception loggingException)
        {
            _logger.LogCritical(loggingException, "Immediate critical crash logging failed.");
            System.Diagnostics.Debug.WriteLine(
                $"[CrashLogger] Failed immediate critical crash save: {loggingException}");
        }
    }

    // Logs critical non-exception messages immediately.
    public void TryLogMessageCriticalImmediately(
        string message,
        string source,
        bool isHandled,
        string severity = "Critical",
        string? pageName = null,
        string? metadataJson = null)
    {
        try
        {
            _logger.LogCritical("Critical crash message logged immediately from {Source}: {Message}", source, message);

            var entry = BuildEntry(
                message: message,
                source: source,
                isHandled: isHandled,
                severity: severity,
                pageName: pageName,
                metadataJson: metadataJson,
                stackTrace: null,
                innerException: null,
                exceptionType: null);

            WriteDirectToSqlite(entry);

            System.Diagnostics.Debug.WriteLine(
                $"[CrashLogger] Critical crash message saved immediately: Id={entry.Id}, Source={entry.Source}, Message={entry.Message}");
        }
        catch (Exception loggingException)
        {
            _logger.LogCritical(loggingException, "Immediate critical crash message logging failed.");
            System.Diagnostics.Debug.WriteLine(
                $"[CrashLogger] Failed immediate critical crash message save: {loggingException}");
        }
    }

    // Builds a CrashLog entity with enriched context information.
    private CrashLog BuildEntry(
        string message,
        string source,
        bool isHandled,
        string severity,
        string? pageName,
        string? metadataJson,
        string? stackTrace,
        string? innerException,
        string? exceptionType)
    {
        return new CrashLog
        {
            Id = Guid.NewGuid(),
            Source = source,
            Severity = severity,
            Message = Truncate(message, 1000),
            StackTrace = stackTrace,
            InnerException = innerException,
            ExceptionType = exceptionType,

            // Session context (user/tenant info).
            AccountId = _sessionService.CurrentAccountId?.ToString(),
            OwnerId = _sessionService.CurrentOwnerId?.ToString(),

            // UI + device context.
            PageName = pageName,
            Platform = DeviceInfo.Platform.ToString(),
            AppVersion = AppInfo.VersionString,
            AppBuild = AppInfo.BuildString,

            // Metadata and flags.
            IsHandled = isHandled,
            IsSynced = false,
            CreatedUtc = DateTime.UtcNow,
            MetadataJson = metadataJson
        };
    }

    // Writes crash log directly to SQLite using raw SQL (fallback path).
    private void WriteDirectToSqlite(CrashLog entry)
    {
        using var connection = new SqliteConnection($"Data Source={_dbPath}");
        connection.Open();

        using var command = connection.CreateCommand();

        command.CommandText =
            """
            INSERT INTO CrashLogs
            (
                Id,
                Source,
                Severity,
                Message,
                StackTrace,
                InnerException,
                ExceptionType,
                AccountId,
                OwnerId,
                PageName,
                Platform,
                AppVersion,
                AppBuild,
                MetadataJson,
                IsHandled,
                IsSynced,
                CreatedUtc
            )
            VALUES
            (
                $Id,
                $Source,
                $Severity,
                $Message,
                $StackTrace,
                $InnerException,
                $ExceptionType,
                $AccountId,
                $OwnerId,
                $PageName,
                $Platform,
                $AppVersion,
                $AppBuild,
                $MetadataJson,
                $IsHandled,
                $IsSynced,
                $CreatedUtc
            );
            """;

        // Parameterised query to prevent SQL injection and ensure type safety.
        command.Parameters.AddWithValue("$Id", entry.Id.ToString());
        command.Parameters.AddWithValue("$Source", entry.Source);
        command.Parameters.AddWithValue("$Severity", entry.Severity);
        command.Parameters.AddWithValue("$Message", entry.Message);
        command.Parameters.AddWithValue("$StackTrace", (object?)entry.StackTrace ?? DBNull.Value);
        command.Parameters.AddWithValue("$InnerException", (object?)entry.InnerException ?? DBNull.Value);
        command.Parameters.AddWithValue("$ExceptionType", (object?)entry.ExceptionType ?? DBNull.Value);
        command.Parameters.AddWithValue("$AccountId", (object?)entry.AccountId ?? DBNull.Value);
        command.Parameters.AddWithValue("$OwnerId", (object?)entry.OwnerId ?? DBNull.Value);
        command.Parameters.AddWithValue("$PageName", (object?)entry.PageName ?? DBNull.Value);
        command.Parameters.AddWithValue("$Platform", entry.Platform);
        command.Parameters.AddWithValue("$AppVersion", entry.AppVersion);
        command.Parameters.AddWithValue("$AppBuild", entry.AppBuild);
        command.Parameters.AddWithValue("$MetadataJson", (object?)entry.MetadataJson ?? DBNull.Value);
        command.Parameters.AddWithValue("$IsHandled", entry.IsHandled ? 1 : 0);
        command.Parameters.AddWithValue("$IsSynced", entry.IsSynced ? 1 : 0);
        command.Parameters.AddWithValue("$CreatedUtc", entry.CreatedUtc);

        command.ExecuteNonQuery();
    }

    // Truncates long messages to avoid excessive storage size.
    private static string Truncate(string value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        return value.Length <= maxLength
            ? value
            : value[..maxLength];
    }
}