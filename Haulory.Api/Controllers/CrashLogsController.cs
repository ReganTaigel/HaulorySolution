using Haulory.Contracts.Diagnostics;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Api.Controllers;

// Marks this as an API controller with automatic validation and binding.
[ApiController]

// Base route: api/crashlogs
[Route("api/[controller]")]
public class CrashLogsController : ControllerBase
{
    #region Dependencies

    // Database context for accessing crash log data.
    private readonly HauloryDbContext _dbContext;

    // Logger used for recording server-side activity.
    private readonly ILogger<CrashLogsController> _logger;

    #endregion

    #region Constructors

    // Constructor injection of dependencies.
    public CrashLogsController(
        HauloryDbContext dbContext,
        ILogger<CrashLogsController> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    #endregion

    #region Endpoints

    // Retrieves the latest 100 crash logs ordered by creation date (newest first).
    // Anonymous access allows mobile apps to query logs without authentication.
    [AllowAnonymous]
    [HttpGet]
    public async Task<ActionResult<List<CrashLogDto>>> GetLatest(CancellationToken cancellationToken)
    {
        var logs = await _dbContext.ServerCrashLogs
            // Order logs so the most recent appear first.
            .OrderByDescending(x => x.CreatedUtc)

            // Limit results to the latest 100 entries.
            .Take(100)

            // Project database entities into DTOs for API response.
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

    // Retrieves a configurable number of recent crash logs.
    // The 'take' parameter is clamped to prevent excessive queries.
    [AllowAnonymous]
    [HttpGet("recent")]
    public async Task<ActionResult<List<CrashLogDto>>> GetRecent(
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        // Ensure the requested amount stays within safe limits (1–500).
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

    // Retrieves a single crash log by its unique identifier.
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CrashLogDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var log = await _dbContext.ServerCrashLogs
            // Filter by the mobile-generated crash ID.
            .Where(x => x.MobileCrashId == id)

            // Map entity to DTO.
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

        // Return 404 if the log does not exist.
        if (log == null)
            return NotFound();

        return Ok(log);
    }

    // Accepts a batch of crash logs from the client (e.g., mobile app sync).
    [AllowAnonymous]
    [HttpPost("bulk")]
    public async Task<IActionResult> BulkInsert(
        [FromBody] SyncCrashLogsRequest request,
        CancellationToken cancellationToken)
    {
        // If no logs are provided, return early.
        if (request?.Logs is null || request.Logs.Count == 0)
            return Ok();

        // Extract incoming crash IDs for duplicate detection.
        var incomingIds = request.Logs.Select(x => x.Id).ToList();

        // Retrieve IDs that already exist in the database.
        var existingIds = await _dbContext.ServerCrashLogs
            .Where(x => incomingIds.Contains(x.MobileCrashId))
            .Select(x => x.MobileCrashId)
            .ToListAsync(cancellationToken);

        // Filter out logs that already exist to avoid duplicates.
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

                // Raw metadata captured from the client (usually JSON).
                MetadataJson = x.MetadataJson,

                // Timestamp when the server received the crash log.
                ReceivedUtc = DateTime.UtcNow
            })
            .ToList();

        // If no new logs remain after filtering, exit early.
        if (newLogs.Count == 0)
            return Ok();

        // Insert new crash logs into the database.
        await _dbContext.ServerCrashLogs.AddRangeAsync(newLogs, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);

        // Log how many crash logs were stored for monitoring purposes.
        _logger.LogInformation("Stored {Count} crash logs.", newLogs.Count);

        return Ok();
    }

    #endregion
}