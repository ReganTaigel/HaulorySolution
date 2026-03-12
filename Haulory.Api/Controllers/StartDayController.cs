using Haulory.Api.Extensions;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/startday")]
[Authorize]
public sealed class StartDayController : ControllerBase
{
    private readonly IVehicleAssetRepository _vehicleRepository;
    private readonly IVehicleDayRunRepository _vehicleDayRunRepository;

    public StartDayController(
        IVehicleAssetRepository vehicleRepository,
        IVehicleDayRunRepository vehicleDayRunRepository)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleDayRunRepository = vehicleDayRunRepository;
    }

    [HttpGet("vehicles")]
    public async Task<IActionResult> GetVehicles()
    {
        System.Diagnostics.Debug.WriteLine("[StartDayController] GET vehicles hit");
        var ownerUserId = User.GetOwnerUserId();

        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

        var result = vehicles
            .OrderBy(v => v.VehicleSetId)
            .ThenBy(v => v.UnitNumber)
            .Select(v => new StartDayVehicleDto
            {
                Id = v.Id,
                VehicleSetId = v.VehicleSetId,
                UnitNumber = v.UnitNumber,
                Rego = v.Rego,
                Make = v.Make,
                Model = v.Model,
                OdometerKm = v.OdometerKm
            })
            .ToList();

        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartVehicleDayRequest request, CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine("[StartDayController] POST start hit");
        if (request is null)
            return BadRequest("Request is required.");

        if (request.VehicleAssetId == Guid.Empty)
            return BadRequest("VehicleAssetId is required.");

        if (request.StartOdometerKm <= 0)
            return BadRequest("StartOdometerKm must be greater than zero.");

        var ownerUserId = User.GetOwnerUserId();
        var userId = User.GetUserId();

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleAssetId);
        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound("Vehicle not found.");

        var run = new VehicleDayRun
        {
            Id = Guid.NewGuid(),
            OwnerUserId = ownerUserId,
            UserId = userId,
            VehicleAssetId = vehicle.Id,
            StartOdometerKm = request.StartOdometerKm,
            StartedAtUtc = DateTime.UtcNow,
            Notes = request.Notes
        };

        await _vehicleDayRunRepository.AddAsync(run, cancellationToken);

        vehicle.OdometerKm = request.StartOdometerKm;
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);

        return Ok(new StartVehicleDayResponse
        {
            Success = true,
            Message = "Start day recorded.",
            RunId = run.Id,
            VehicleAssetId = run.VehicleAssetId,
            StartOdometerKm = run.StartOdometerKm,
            StartedAtUtc = run.StartedAtUtc
        });
    }

    [HttpPost("finish")]
    public async Task<IActionResult> Finish([FromBody] FinishVehicleDayRequest request, CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine("[StartDayController] POST finish hit");
        if (request is null)
            return BadRequest("Request is required.");

        if (request.VehicleAssetId == Guid.Empty)
            return BadRequest("VehicleAssetId is required.");

        if (request.EndOdometerKm <= 0)
            return BadRequest("EndOdometerKm must be greater than zero.");

        var userId = User.GetUserId();

        var run = await _vehicleDayRunRepository.GetLatestByUserAndVehicleAsync(
            userId,
            request.VehicleAssetId,
            cancellationToken);

        if (run is null)
            return NotFound("No day run found for this vehicle.");

        run.EndOdometerKm = request.EndOdometerKm;
        run.FinishedAtUtc = DateTime.UtcNow;
        run.Notes = string.IsNullOrWhiteSpace(request.Notes) ? run.Notes : request.Notes;

        await _vehicleDayRunRepository.UpdateAsync(run, cancellationToken);

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleAssetId);
        if (vehicle is not null)
        {
            vehicle.OdometerKm = request.EndOdometerKm;
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        }

        return Ok(new FinishVehicleDayResponse
        {
            Success = true,
            Message = "Finish day recorded.",
            RunId = run.Id,
            VehicleAssetId = run.VehicleAssetId,
            StartOdometerKm = run.StartOdometerKm,
            EndOdometerKm = run.EndOdometerKm.Value,
            FinishedAtUtc = run.FinishedAtUtc!.Value
        });
    }
}

public sealed class StartDayVehicleDto
{
    public Guid Id { get; set; }
    public Guid VehicleSetId { get; set; }
    public int UnitNumber { get; set; }
    public string Rego { get; set; } = string.Empty;
    public string Make { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public int? OdometerKm { get; set; }
}

public sealed class StartVehicleDayRequest
{
    public Guid VehicleAssetId { get; set; }
    public int StartOdometerKm { get; set; }
    public string? Notes { get; set; }
}

public sealed class StartVehicleDayResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid RunId { get; set; }
    public Guid VehicleAssetId { get; set; }
    public int StartOdometerKm { get; set; }
    public DateTime StartedAtUtc { get; set; }
}

public sealed class FinishVehicleDayRequest
{
    public Guid VehicleAssetId { get; set; }
    public int EndOdometerKm { get; set; }
    public string? Notes { get; set; }
}

public sealed class FinishVehicleDayResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid RunId { get; set; }
    public Guid VehicleAssetId { get; set; }
    public int StartOdometerKm { get; set; }
    public int EndOdometerKm { get; set; }
    public DateTime FinishedAtUtc { get; set; }
}