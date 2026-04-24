using HaulitCore.Api.Extensions;
using HaulitCore.Contracts.Vehicles;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;
using HaulitCore.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaulitCore.Api.Controllers;

// Marks this as an API controller with automatic model binding and validation.
[ApiController]

// Base route: api/Hubodometer
[Route("api/Hubodometer")]

// Requires authentication for all endpoints.
[Authorize]
public sealed class HubodometerController : ControllerBase
{
    // Repository for vehicle asset data.
    private readonly IVehicleAssetRepository _vehicleRepository;

    // Repository for tracking daily vehicle runs.
    private readonly IVehicleDayRunRepository _vehicleDayRunRepository;

    // Direct DbContext access for writing Hubodometer readings.
    private readonly HaulitCoreDbContext _context;

    // Constructor injection of dependencies.
    public HubodometerController(
        IVehicleAssetRepository vehicleRepository,
        IVehicleDayRunRepository vehicleDayRunRepository,
        HaulitCoreDbContext context)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleDayRunRepository = vehicleDayRunRepository;
        _context = context;
    }

    // Retrieves all vehicle assets for the current owner, including Hubodometer values.
    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets(CancellationToken cancellationToken)
    {
        // Debug logging to confirm endpoint execution.
        System.Diagnostics.Debug.WriteLine("[HubodometerController] GET assets hit");

        // Extract owner ID from authenticated user.
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve all vehicles belonging to the owner.
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

        // Sort vehicles and map to DTOs.
        var result = vehicles
            .OrderBy(v => v.VehicleSetId)
            .ThenBy(v => v.UnitNumber)
            .Select(v => new HubodometerAssetDto
            {
                Id = v.Id,
                VehicleSetId = v.VehicleSetId,
                UnitNumber = v.UnitNumber,
                Rego = v.Rego,
                Make = v.Make,
                Model = v.Model,
                HubodometerKm = v.HubodometerKm
            })
            .ToList();

        return Ok(result);
    }

    // Records a new Hubodometer reading for a vehicle.
    [HttpPost("readings")]
    public async Task<IActionResult> RecordReading(
        [FromBody] HubodometerReadingRequest request,
        CancellationToken cancellationToken)
    {
        // Debug logging to confirm endpoint execution.
        System.Diagnostics.Debug.WriteLine("[HubodometerController] POST readings hit");

        // Validate request presence.
        if (request is null)
            return BadRequest("Request is required.");

        // Validate required fields.
        if (request.VehicleAssetId == Guid.Empty)
            return BadRequest("VehicleAssetId is required.");

        if (request.ReadingKm <= 0)
            return BadRequest("ReadingKm must be greater than zero.");

        // Extract ownership and user context from claims.
        var ownerUserId = User.GetOwnerUserId();
        var userId = User.GetUserId();

        // Retrieve vehicle and ensure it belongs to the current owner.
        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleAssetId);
        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound("Vehicle not found.");

        // Create a new Hubodometer reading entity.
        var reading = new HubodometerReading(
            request.VehicleAssetId,
            vehicle.UnitNumber,
            request.ReadingKm,
            request.ReadingType,
            request.DriverId,
            request.RecordedByUserId ?? userId,
            request.Notes);

        // Persist the reading directly using DbContext.
        _context.HubodometerReadings.Add(reading);
        await _context.SaveChangesAsync(cancellationToken);

        // Optionally update the vehicle's current Hubodometer value.
        if (request.UpdateCurrentHubodometer)
        {
            vehicle.HubodometerKm = request.ReadingKm;
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        }

        // If this is a start-of-day reading, create a new vehicle run record.
        if (request.ReadingType == HubodometerReadingType.StartOfDay)
        {
            var run = new VehicleDayRun
            {
                Id = Guid.NewGuid(),
                OwnerUserId = ownerUserId,
                UserId = userId,
                VehicleAssetId = request.VehicleAssetId,
                StartHubodometerKm = request.ReadingKm,
                StartedAtUtc = DateTime.UtcNow,
                Notes = request.Notes
            };

            await _vehicleDayRunRepository.AddAsync(run, cancellationToken);
        }
        // If this is an end-of-day reading, complete the latest run.
        else if (request.ReadingType == HubodometerReadingType.EndOfDay)
        {
            // Retrieve the most recent run for this user and vehicle.
            var run = await _vehicleDayRunRepository.GetLatestByUserAndVehicleAsync(
                userId,
                request.VehicleAssetId,
                cancellationToken);

            if (run is not null)
            {
                // Set end-of-day values.
                run.EndHubodometerKm = request.ReadingKm;
                run.FinishedAtUtc = DateTime.UtcNow;

                // Update notes only if new notes were provided.
                run.Notes = string.IsNullOrWhiteSpace(request.Notes) ? run.Notes : request.Notes;

                await _vehicleDayRunRepository.UpdateAsync(run, cancellationToken);
            }
        }

        // Return success response.
        return Ok(new HubodometerReadingResponse
        {
            Success = true,
            Message = "Hubodometer reading recorded successfully."
        });
    }
}