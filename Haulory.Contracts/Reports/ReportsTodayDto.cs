namespace Haulory.Contracts.Reports;

public sealed class ReportsTodayDto
{
    public int CompletedTodayCount { get; set; }
    public decimal RevenueToday { get; set; }
    public string? LatestReferenceNumber { get; set; }
    public string? LatestReceiver { get; set; }
    public decimal? LatestTotal { get; set; }
}