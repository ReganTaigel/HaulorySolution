namespace Haulory.Contracts.Diagnostics;

public sealed class SyncCrashLogsRequest
{
    #region Properties

    public List<CrashLogDto> Logs { get; set; } = new();

    #endregion
}