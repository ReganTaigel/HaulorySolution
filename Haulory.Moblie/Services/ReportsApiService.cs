using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Contracts.Reports;

namespace Haulory.Mobile.Services;

public sealed class ReportsApiService : ApiServiceBase
{
    public ReportsApiService(HttpClient httpClient, ISessionService sessionService)
        : base(httpClient, sessionService)
    {
    }

    public async Task<ReportsTodayDto> GetTodayAsync(CancellationToken cancellationToken = default)
    {
        var result = await GetAsync<ReportsTodayDto>("api/reports/today", cancellationToken);
        return result ?? new ReportsTodayDto();
    }

    public async Task<IReadOnlyList<DeliveryReceiptDto>> GetReceiptsAsync(
        DateTime date,
        CancellationToken cancellationToken = default)
    {
        var uri = $"api/reports/receipts?date={date:yyyy-MM-dd}";
        return await GetAsync<List<DeliveryReceiptDto>>(uri, cancellationToken) ?? new List<DeliveryReceiptDto>();
    }
}