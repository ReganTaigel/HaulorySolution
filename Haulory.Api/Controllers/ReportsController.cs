using Haulory.Contracts.Reports;
using Haulory.Api.Extensions;
using Haulory.Application.Features.Reports;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

// Marks this class as an API controller with automatic binding and validation support.
[ApiController]

// Base route: api/reports
[Route("api/reports")]

// Requires authentication for all reporting endpoints.
[Authorize]
public class ReportsController : ControllerBase
{
    // Repository used to retrieve delivery receipt data.
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;

    // Handler responsible for building invoice report DTOs.
    private readonly InvoiceReportHandler _invoiceReportHandler;

    // Handler responsible for building POD report DTOs.
    private readonly PodReportHandler _podReportHandler;

    // Service used to generate invoice PDF files.
    private readonly IPdfInvoiceGenerator _pdfInvoiceGenerator;

    // Service used to generate POD PDF files.
    private readonly IPdfPodGenerator _pdfPodGenerator;

    // Constructor injection of required reporting dependencies.
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

    // Returns summary metrics for the current local day.
    [HttpGet("today")]
    [ProducesResponseType(typeof(ReportsTodayDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportsTodayDto>> GetToday()
    {
        // Get the owner ID from the authenticated user's claims.
        var ownerUserId = User.GetOwnerUserId();

        // Build a UTC range representing the current local day.
        var todayLocal = DateTime.Today;
        var (fromUtc, toUtc) = BuildUtcRangeForLocalDay(todayLocal);

        // Retrieve all delivery receipts completed within today's UTC range.
        var todayReceipts = await _deliveryReceiptRepository.GetByOwnerDeliveredBetweenUtcAsync(
            ownerUserId,
            fromUtc,
            toUtc);

        // Identify the most recently delivered receipt for display purposes.
        var latest = todayReceipts
            .OrderByDescending(r => r.DeliveredAtUtc)
            .FirstOrDefault();

        // Return a dashboard-style summary for today.
        return Ok(new ReportsTodayDto
        {
            CompletedTodayCount = todayReceipts.Count,
            RevenueToday = todayReceipts.Sum(r => r.Total),
            LatestReferenceNumber = latest?.ReferenceNumber,
            LatestReceiver = latest?.ReceiverName,
            LatestTotal = latest?.Total
        });
    }

    // Returns delivery receipts for a specified local date, or today if no date is provided.
    [HttpGet("receipts")]
    [ProducesResponseType(typeof(IEnumerable<DeliveryReceiptDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DeliveryReceiptDto>>> GetReceiptsByDate([FromQuery] DateTime? date = null)
    {
        // Get the owner ID from the authenticated user's claims.
        var ownerUserId = User.GetOwnerUserId();

        // Use the requested date or default to the current local date.
        var targetDate = (date ?? DateTime.Today).Date;

        // Build a UTC range for the requested local date.
        var (fromUtc, toUtc) = BuildUtcRangeForLocalDay(targetDate);

        // Retrieve all receipts delivered during that date range.
        var receipts = await _deliveryReceiptRepository.GetByOwnerDeliveredBetweenUtcAsync(
            ownerUserId,
            fromUtc,
            toUtc);

        // Order newest first and map to DTOs.
        var filtered = receipts
            .OrderByDescending(r => r.DeliveredAtUtc)
            .Select(r => r.ToDto())
            .ToList();

        return Ok(filtered);
    }

    // Generates and returns an invoice PDF for a given receipt.
    [HttpGet("invoices/{receiptId:guid}/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> ExportInvoicePdf(
        Guid receiptId,
        [FromQuery] bool includeGst = true,
        [FromQuery] decimal gstRate = 0.15m)
    {
        // Get the owner ID from the authenticated user's claims.
        var ownerUserId = User.GetOwnerUserId();

        // Build the invoice report DTO using the report handler.
        var dto = await _invoiceReportHandler.HandleAsync(ownerUserId, receiptId, includeGst, gstRate);

        // Generate the invoice PDF bytes.
        var pdfBytes = _pdfInvoiceGenerator.GenerateInvoicePdf(dto, Array.Empty<byte>());

        // Build a safe fallback file name if the invoice number is missing.
        var safeInvoice = string.IsNullOrWhiteSpace(dto.InvoiceNumber)
            ? "invoice"
            : dto.InvoiceNumber.Trim();

        var fileName = $"Invoice_{safeInvoice}.pdf";

        // Return the PDF file to the client.
        return File(pdfBytes, "application/pdf", fileName);
    }

    // Generates and returns a proof-of-delivery PDF for a given receipt.
    [HttpGet("pods/{receiptId:guid}/pdf")]
    [Produces("application/pdf")]
    public async Task<IActionResult> ExportPodPdf(Guid receiptId)
    {
        // Debug logging to help trace PDF export requests during development.
        System.Diagnostics.Debug.WriteLine(
    $"[ExportPodPdf] ReceiptId={receiptId}");

        // Get the owner ID from the authenticated user's claims.
        var ownerUserId = User.GetOwnerUserId();

        // Build the POD report DTO using the report handler.
        var dto = await _podReportHandler.HandleAsync(ownerUserId, receiptId);

        // Generate the POD PDF bytes.
        var pdfBytes = _pdfPodGenerator.GeneratePodPdf(dto);

        // Build a safe fallback file name if the reference number is missing.
        var safeRef = string.IsNullOrWhiteSpace(dto.ReferenceNumber)
            ? "pod"
            : dto.ReferenceNumber.Trim();

        var fileName = $"POD_{safeRef}.pdf";

        // Return the PDF file to the client.
        return File(pdfBytes, "application/pdf", fileName);
    }

    // Converts a local calendar date into a UTC start/end range for querying persisted UTC data.
    private static (DateTime fromUtc, DateTime toUtc) BuildUtcRangeForLocalDay(DateTime localDate)
    {
        // Set the beginning of the local day.
        var localStart = DateTime.SpecifyKind(localDate.Date, DateTimeKind.Local);

        // Set the beginning of the following local day.
        var localEnd = localStart.AddDays(1);

        // Convert the local range to UTC for database queries.
        var fromUtc = localStart.ToUniversalTime();
        var toUtc = localEnd.ToUniversalTime();

        return (fromUtc, toUtc);
    }
}