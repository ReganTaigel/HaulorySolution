using HaulitCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace HaulitCore.Api.Controllers;

// Marks this as an API controller with automatic model binding and validation.
[ApiController]

// Base route: api/test
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    // Database context used for connectivity and basic data checks.
    private readonly HaulitCoreDbContext _db;

    // Constructor injection of the DbContext.
    public TestController(HaulitCoreDbContext db)
    {
        _db = db;
    }

    // Simple endpoint to confirm the API is running.
    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("HaulitCore API is running.");
    }

    // Performs a basic database connectivity and data presence check.
    [HttpGet("db-check")]
    public async Task<IActionResult> DbCheck()
    {
        // Check if the application can establish a connection to the database.
        var canConnect = await _db.Database.CanConnectAsync();

        // Return connection status along with record counts for key tables.
        return Ok(new
        {
            canConnect,

            // Basic counts to verify data access and schema integrity.
            userAccounts = await _db.UserAccounts.CountAsync(),
            drivers = await _db.Drivers.CountAsync(),
            jobs = await _db.Jobs.CountAsync(),
            vehicles = await _db.VehicleAssets.CountAsync()
        });
    }
}