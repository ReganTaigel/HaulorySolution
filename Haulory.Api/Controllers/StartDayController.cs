using Haulory.Api.Extensions;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

// Marks this as an API controller with automatic model binding and validation.
[ApiController]

// Base route: api/startday
[Route("api/startday")]

// Requires authentication for all endpoints.
[Authorize]
public sealed class StartDayController : ControllerBase
{
    // Repository for vehicle assets.
    private readonly IVehicleAssetRepository _vehicleRepository;

    // Repository for tracking vehicle day runs (start → finish lifecycle).
    private readonly IVehicleDayRunRepository _vehicleDayRunRepository;

    // Constructor injection of dependencies.
    public StartDayController(
        IVehicleAssetRepository vehicleRepository,
        IVehicleDayRunRepository vehicleDayRunRepository)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleDayRunRepository = vehicleDayRunRepository;
    }

    // Returns available vehicles for starting a day run.
    [HttpGet("vehicles")]
    public async Task<IActionResult> GetVehicles()
    {
        // Debug logging to confirm endpoint execution.
        System.Diagnostics.Debug.WriteLine("[StartDayController] GET vehicles hit");

        // Extract owner ID from authenticated user.
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve all vehicles belonging to the owner.
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

        // Sort and map vehicles into DTOs for UI consumption.
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

    // Starts a vehicle day run (beginning of a driver's workday).
    [HttpPost("start")]
    public async Task<IActionResult> Start([FromBody] StartVehicleDayRequest request, CancellationToken cancellationToken)
    {
        // Debug logging.
        System.Diagnostics.Debug.WriteLine("[StartDayController] POST start hit");

        // Validate request presence.
        if (request is null)
            return BadRequest("Request is required.");

        // Validate required fields.
        if (request.VehicleAssetId == Guid.Empty)
            return BadRequest("VehicleAssetId is required.");

        if (request.StartOdometerKm <= 0)
            return BadRequest("StartOdometerKm must be greater than zero.");

        // Extract ownership and user context.
        var ownerUserId = User.GetOwnerUserId();
        var userId = User.GetUserId();

        // Retrieve vehicle and verify ownership.
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleAssetId);
        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound("Vehicle not found.");

        // Create a new day run record.
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

        // Persist the new run.
        await _vehicleDayRunRepository.AddAsync(run, cancellationToken);

        // Update vehicle's current odometer reading.
        vehicle.OdometerKm = request.StartOdometerKm;
        await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);

        // Return success response with run details.
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

    // Finishes a vehicle day run (end of a driver's workday).
    [HttpPost("finish")]
    public async Task<IActionResult> Finish([FromBody] FinishVehicleDayRequest request, CancellationToken cancellationToken)
    {
        // Debug logging.
        System.Diagnostics.Debug.WriteLine("[StartDayController] POST finish hit");

        // Validate request presence.
        if (request is null)
            return BadRequest("Request is required.");

        // Validate required fields.
        if (request.VehicleAssetId == Guid.Empty)
            return BadRequest("VehicleAssetId is required.");

        if (request.EndOdometerKm <= 0)
            return BadRequest("EndOdometerKm must be greater than zero.");

        // Extract user context.
        var userId = User.GetUserId();

        // Retrieve the latest run for this user and vehicle.
        var run = await _vehicleDayRunRepository.GetLatestByUserAndVehicleAsync(
            userId,
            request.VehicleAssetId,
            cancellationToken);

        if (run is null)
            return NotFound("No day run found for this vehicle.");

        // Update run with end-of-day values.
        run.EndOdometerKm = request.EndOdometerKm;
        run.FinishedAtUtc = DateTime.UtcNow;

        // Update notes only if new notes were provided.
        run.Notes = string.IsNullOrWhiteSpace(request.Notes) ? run.Notes : request.Notes;

        // Persist updated run.
        await _vehicleDayRunRepository.UpdateAsync(run, cancellationToken);

        // Update vehicle's current odometer reading.
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleAssetId);
        if (vehicle is not null)
        {
            vehicle.OdometerKm = request.EndOdometerKm;
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        }

        // Return success response with run details.
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

// DTO representing a vehicle available for starting a day.
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

// Request payload for starting a vehicle day.
public sealed class StartVehicleDayRequest
{
    public Guid VehicleAssetId { get; set; }
    public int StartOdometerKm { get; set; }
    public string? Notes { get; set; }
}

// Response returned after successfully starting a day.
public sealed class StartVehicleDayResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public Guid RunId { get; set; }
    public Guid VehicleAssetId { get; set; }
    public int StartOdometerKm { get; set; }
    public DateTime StartedAtUtc { get; set; }
}

// Request payload for finishing a vehicle day.
public sealed class FinishVehicleDayRequest
{
    public Guid VehicleAssetId { get; set; }
    public int EndOdometerKm { get; set; }
    public string? Notes { get; set; }
}

// Response returned after successfully finishing a day.
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