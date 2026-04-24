using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Contracts.Reports;

namespace HaulitCore.Mobile.Services;

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
        var localDate = date.Date;
        var uri = $"api/reports/receipts?date={localDate:yyyy-MM-dd}";

        return await GetAsync<List<DeliveryReceiptDto>>(uri, cancellationToken)
               ?? new List<DeliveryReceiptDto>();
    }

    public async Task<byte[]> ExportInvoicePdfAsync(
        Guid receiptId,
        bool includeGst,
        decimal gstRate,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var uri = $"api/reports/invoices/{receiptId}/pdf?includeGst={includeGst}&gstRate={gstRate}";
        var response = await HttpClient.GetAsync(uri, cancellationToken);

        var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to export invoice PDF. Status: {(int)response.StatusCode}");

        return body;
    }

    public async Task<byte[]> ExportPodPdfAsync(
        Guid receiptId,
        CancellationToken cancellationToken = default)
    {
        await EnsureAuthenticatedAsync();

        var response = await HttpClient.GetAsync($"api/reports/pods/{receiptId}/pdf", cancellationToken);
        var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);

        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Failed to export POD PDF. Status: {(int)response.StatusCode}");

        return body;
    }
}