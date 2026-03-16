using Haulory.Contracts.Reports;
using Haulory.Api.Extensions;
using Haulory.Application.Features.Reports;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly InvoiceReportHandler _invoiceReportHandler;
    private readonly PodReportHandler _podReportHandler;
    private readonly IPdfInvoiceGenerator _pdfInvoiceGenerator;
    private readonly IPdfPodGenerator _pdfPodGenerator;

    public ReportsController(
        IDeliveryReceiptRepository deliveryReceiptRepository,
        InvoiceReportHandler invoiceReportHandler,
        PodReportHandler podReportHandler,
        IPdfInvoiceGenerator pdfInvoiceGenerator,
        IPdfPodGenerator pdfPodGenerator)
    {
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _invoiceReportHandler = invoiceReportHandler;
        _podReportHandler = podReportHandler;
        _pdfInvoiceGenerator = pdfInvoiceGenerator;
        _pdfPodGenerator = pdfPodGenerator;
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(ReportsTodayDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportsTodayDto>> GetToday()
    {
        var ownerUserId = User.GetOwnerUserId();

        var todayLocal = DateTime.Today;
        var (fromUtc, toUtc) = BuildUtcRangeForLocalDay(todayLocal);

        var todayReceipts = await _deliveryReceiptRepository.GetByOwnerDeliveredBetweenUtcAsync(
            ownerUserId,
            fromUtc,
            toUtc);

        var latest = todayReceipts
            .OrderByDescending(r => r.DeliveredAtUtc)
            .FirstOrDefault();

        return Ok(new ReportsTodayDto
        {
            CompletedTodayCount = todayReceipts.Count,
            RevenueToday = todayReceipts.Sum(r => r.Total),
            LatestReferenceNumber = latest?.ReferenceNumber,
            LatestReceiver = latest?.ReceiverName,
            LatestTotal = latest?.Total
        });
    }

    [HttpGet("receipts")]
    [ProducesResponseType(typeof(IEnumerable<DeliveryReceiptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeliveryReceiptDto>>> GetReceiptsByDate([FromQuery] DateTime? date = null)
    {
        var ownerUserId = User.GetOwnerUserId();

        var targetDate = (date ?? DateTime.Today).Date;
        var (fromUtc, toUtc) = BuildUtcRangeForLocalDay(targetDate);

        var receipts = await _deliveryReceiptRepository.GetByOwnerDeliveredBetweenUtcAsync(
            ownerUserId,
            fromUtc,
            toUtc);

        var filtered = receipts
            .OrderByDescending(r => r.DeliveredAtUtc)
            .Select(r => r.ToDto())
            .ToList();

        return Ok(filtered);
    }

    [HttpGet("invoices/{receiptId:guid}/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> ExportInvoicePdf(
        Guid receiptId,
        [FromQuery] bool includeGst = true,
        [FromQuery] decimal gstRate = 0.15m)
    {
        var ownerUserId = User.GetOwnerUserId();

        var dto = await _invoiceReportHandler.HandleAsync(ownerUserId, receiptId, includeGst, gstRate);
        var pdfBytes = _pdfInvoiceGenerator.GenerateInvoicePdf(dto, Array.Empty<byte>());

        var safeInvoice = string.IsNullOrWhiteSpace(dto.InvoiceNumber)
            ? "invoice"
            : dto.InvoiceNumber.Trim();

        var fileName = $"Invoice_{safeInvoice}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    [HttpGet("pods/{receiptId:guid}/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> ExportPodPdf(Guid receiptId)
    {
        var ownerUserId = User.GetOwnerUserId();

        var dto = await _podReportHandler.HandleAsync(ownerUserId, receiptId);
        var pdfBytes = _pdfPodGenerator.GeneratePodPdf(dto);

        var safeRef = string.IsNullOrWhiteSpace(dto.ReferenceNumber)
            ? "pod"
            : dto.ReferenceNumber.Trim();

        var fileName = $"POD_{safeRef}.pdf";

        return File(pdfBytes, "application/pdf", fileName);
    }

    private static (DateTime fromUtc, DateTime toUtc) BuildUtcRangeForLocalDay(DateTime localDate)
    {
        var localStart = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Local);
        var localEnd = localStart.AddDays(1);

        var fromUtc = localStart.ToUniversalTime();
        var toUtc = localEnd.ToUniversalTime();

        return (fromUtc, toUtc);
    }
}