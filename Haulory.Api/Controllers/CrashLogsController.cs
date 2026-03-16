using Haulory.Contracts.Diagnostics;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CrashLogsController : ControllerBase
{
    #region Dependencies

    private readonly HauloryDbContext _dbContext;
    private readonly ILogger<CrashLogsController> _logger;

    #endregion

    #region Constructors

    public CrashLogsController(
        HauloryDbContext dbContext,
        ILogger<CrashLogsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #endregion

    #region Endpoints

    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<CrashLogDto>>> GetLatest(CancellationToken cancellationToken)
    {
        var logs = await _dbContext.ServerCrashLogs
            .OrderByDescending(x => x.CreatedUtc)
            .Take(100)
            .Select(x => new CrashLogDto
            {
                Id = x.MobileCrashId,
                Source = x.Source,
                Severity = x.Severity,
                Message = x.Message,
                StackTrace = x.StackTrace,
                InnerException = x.InnerException,
                ExceptionType = x.ExceptionType,
                AccountId = x.AccountId,
                OwnerId = x.OwnerId,
                PageName = x.PageName,
                Platform = x.Platform,
                AppVersion = x.AppVersion,
                AppBuild = x.AppBuild,
                IsHandled = x.IsHandled,
                CreatedUtc = x.CreatedUtc,
                MetadataJson = x.MetadataJson
            })
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }

    [AllowAnonymous]
    [HttpGet("recent")]
    public async Task<ActionResult<List<CrashLogDto>>> GetRecent(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 500);

        var logs = await _dbContext.ServerCrashLogs
            .OrderByDescending(x => x.CreatedUtc)
            .Take(take)
            .Select(x => new CrashLogDto
            {
                Id = x.MobileCrashId,
                Source = x.Source,
                Severity = x.Severity,
                Message = x.Message,
                StackTrace = x.StackTrace,
                InnerException = x.InnerException,
                ExceptionType = x.ExceptionType,
                AccountId = x.AccountId,
                OwnerId = x.OwnerId,
                PageName = x.PageName,
                Platform = x.Platform,
                AppVersion = x.AppVersion,
                AppBuild = x.AppBuild,
                IsHandled = x.IsHandled,
                CreatedUtc = x.CreatedUtc,
                MetadataJson = x.MetadataJson
            })
            .ToListAsync(cancellationToken);

        return Ok(logs);
    }

    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CrashLogDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var log = await _dbContext.ServerCrashLogs
            .Where(x => x.MobileCrashId == id)
            .Select(x => new CrashLogDto
            {
                Id = x.MobileCrashId,
                Source = x.Source,
                Severity = x.Severity,
                Message = x.Message,
                StackTrace = x.StackTrace,
                InnerException = x.InnerException,
                ExceptionType = x.ExceptionType,
                AccountId = x.AccountId,
                OwnerId = x.OwnerId,
                PageName = x.PageName,
                Platform = x.Platform,
                AppVersion = x.AppVersion,
                AppBuild = x.AppBuild,
                IsHandled = x.IsHandled,
                CreatedUtc = x.CreatedUtc,
                MetadataJson = x.MetadataJson
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (log == null)
            return NotFound();

        return Ok(log);
    }

    [AllowAnonymous]
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkInsert(
        [FromBody] SyncCrashLogsRequest request,
        CancellationToken cancellationToken)
    {
        if (request?.Logs is null || request.Logs.Count == 0)
            return Ok();

        var incomingIds = request.Logs.Select(x => x.Id).ToList();

        var existingIds = await _dbContext.ServerCrashLogs
            .Where(x => incomingIds.Contains(x.MobileCrashId))
            .Select(x => x.MobileCrashId)
            .ToListAsync(cancellationToken);

        var newLogs = request.Logs
            .Where(x => !existingIds.Contains(x.Id))
            .Select(x => new ServerCrashLog
            {
                MobileCrashId = x.Id,
                Source = x.Source,
                Severity = x.Severity,
                Message = x.Message,
                StackTrace = x.StackTrace,
                InnerException = x.InnerException,
                ExceptionType = x.ExceptionType,
                AccountId = x.AccountId,
                OwnerId = x.OwnerId,
                PageName = x.PageName,
                Platform = x.Platform,
                AppVersion = x.AppVersion,
                AppBuild = x.AppBuild,
                IsHandled = x.IsHandled,
                CreatedUtc = x.CreatedUtc,
                MetadataJson = x.MetadataJson,
                ReceivedUtc = DateTime.UtcNow
            })
            .ToList();

        if (newLogs.Count == 0)
            return Ok();

        await _dbContext.ServerCrashLogs.AddRangeAsync(newLogs, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Stored {Count} crash logs.", newLogs.Count);

        return Ok();
    }

    #endregion
}