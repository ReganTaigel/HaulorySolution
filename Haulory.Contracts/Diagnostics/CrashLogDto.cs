namespace Haulory.Contracts.Diagnostics;

public sealed class CrashLogDto
{
    public Guid Id { get; set; }

    public string Source { get; set; } = string.Empty;

    public string Severity { get; set; } = "Error";

    public string Message { get; set; } = string.Empty;

    public string? StackTrace { get; set; }

    public string? InnerException { get; set; }

    public string? ExceptionType { get; set; }

    public string? AccountId { get; set; }

    public string? OwnerId { get; set; }

    public string? PageName { get; set; }

    public string Platform { get; set; } = string.Empty;

    public string AppVersion { get; set; } = string.Empty;

    public string AppBuild { get; set; } = string.Empty;

    public bool IsHandled { get; set; }

    public DateTime CreatedUtc { get; set; }

    public string? MetadataJson { get; set; }
}