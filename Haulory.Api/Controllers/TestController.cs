using Haulory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly HauloryDbContext _db;

    public TestController(HauloryDbContext db)
    {
        _db = db;
    }

    [HttpGet("ping")]
    public IActionResult Ping()
    {
        return Ok("Haulory API is running.");
    }

    [HttpGet("db-check")]
    public async Task<IActionResult> DbCheck()
    {
        var canConnect = await _db.Database.CanConnectAsync();

        return Ok(new
        {
            canConnect,
            userAccounts = await _db.UserAccounts.CountAsync(),
            drivers = await _db.Drivers.CountAsync(),
            jobs = await _db.Jobs.CountAsync(),
            vehicles = await _db.VehicleAssets.CountAsync()
        });
    }
}