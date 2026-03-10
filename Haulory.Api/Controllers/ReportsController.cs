using System.Security.Claims;
using Haulory.Api.Contracts.Reports;
using Haulory.Api.Extensions;
using Haulory.Application.Interfaces.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/reports")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;

    public ReportsController(IDeliveryReceiptRepository deliveryReceiptRepository)
    {
        _deliveryReceiptRepository = deliveryReceiptRepository;
    }

    [HttpGet("today")]
    [ProducesResponseType(typeof(ReportsTodayDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<ReportsTodayDto>> GetToday()
    {
        var ownerUserId = GetOwnerUserId();

        var receipts = await _deliveryReceiptRepository.GetByOwnerAsync(ownerUserId);

        var todayLocal = DateTime.Now.Date;

        var todayReceipts = receipts
            .Where(r => ToLocalDate(r.DeliveredAtUtc) == todayLocal)
            .OrderByDescending(r => r.DeliveredAtUtc)
            .ToList();

        var latest = todayReceipts.FirstOrDefault();

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
        var ownerUserId = GetOwnerUserId();

        var targetDate = (date ?? DateTime.Now).Date;
        var receipts = await _deliveryReceiptRepository.GetByOwnerAsync(ownerUserId);

        var filtered = receipts
            .Where(r => ToLocalDate(r.DeliveredAtUtc) == targetDate)
            .OrderByDescending(r => r.DeliveredAtUtc)
            .Select(r => r.ToDto())
            .ToList();

        return Ok(filtered);
    }

    private Guid GetOwnerUserId()
    {
        var value =
            User.FindFirstValue("owner_id") ??
            User.FindFirstValue("ownerId") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId");

        if (!Guid.TryParse(value, out var ownerUserId))
            throw new UnauthorizedAccessException("Authenticated owner id is missing or invalid.");

        return ownerUserId;
    }

    private static DateTime ToLocalDate(DateTime deliveredAtUtc)
    {
        var utc = deliveredAtUtc.Kind == DateTimeKind.Utc
            ? deliveredAtUtc
            : DateTime.SpecifyKind(deliveredAtUtc, DateTimeKind.Utc);

        return utc.ToLocalTime().Date;
    }
}