using Haulory.Api.Extensions;
using Haulory.Contracts.Vehicles;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Haulory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

// Marks this as an API controller with automatic model binding and validation.
[ApiController]

// Base route: api/odometer
[Route("api/odometer")]

// Requires authentication for all endpoints.
[Authorize]
public sealed class OdometerController : ControllerBase
{
    // Repository for vehicle asset data.
    private readonly IVehicleAssetRepository _vehicleRepository;

    // Repository for tracking daily vehicle runs.
    private readonly IVehicleDayRunRepository _vehicleDayRunRepository;

    // Direct DbContext access for writing odometer readings.
    private readonly HauloryDbContext _context;

    // Constructor injection of dependencies.
    public OdometerController(
        IVehicleAssetRepository vehicleRepository,
        IVehicleDayRunRepository vehicleDayRunRepository,
        HauloryDbContext context)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleDayRunRepository = vehicleDayRunRepository;
        _context = context;
    }

    // Retrieves all vehicle assets for the current owner, including odometer values.
    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets(CancellationToken cancellationToken)
    {
        // Debug logging to confirm endpoint execution.
        System.Diagnostics.Debug.WriteLine("[OdometerController] GET assets hit");

        // Extract owner ID from authenticated user.
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve all vehicles belonging to the owner.
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

        // Sort vehicles and map to DTOs.
        var result = vehicles
            .OrderBy(v => v.VehicleSetId)
            .ThenBy(v => v.UnitNumber)
            .Select(v => new OdometerAssetDto
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

    // Records a new odometer reading for a vehicle.
    [HttpPost("readings")]
    public async Task<IActionResult> RecordReading(
        [FromBody] OdometerReadingRequest request,
        CancellationToken cancellationToken)
    {
        // Debug logging to confirm endpoint execution.
        System.Diagnostics.Debug.WriteLine("[OdometerController] POST readings hit");

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

        // Create a new odometer reading entity.
        var reading = new OdometerReading(
            request.VehicleAssetId,
            vehicle.UnitNumber,
            request.ReadingKm,
            request.ReadingType,
            request.DriverId,
            request.RecordedByUserId ?? userId,
            request.Notes);

        // Persist the reading directly using DbContext.
        _context.OdometerReadings.Add(reading);
        await _context.SaveChangesAsync(cancellationToken);

        // Optionally update the vehicle's current odometer value.
        if (request.UpdateCurrentOdometer)
        {
            vehicle.OdometerKm = request.ReadingKm;
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        }

        // If this is a start-of-day reading, create a new vehicle run record.
        if (request.ReadingType == OdometerReadingType.StartOfDay)
        {
            var run = new VehicleDayRun
            {
                Id = Guid.NewGuid(),
                OwnerUserId = ownerUserId,
                UserId = userId,
                VehicleAssetId = request.VehicleAssetId,
                StartOdometerKm = request.ReadingKm,
                StartedAtUtc = DateTime.UtcNow,
                Notes = request.Notes
            };

            await _vehicleDayRunRepository.AddAsync(run, cancellationToken);
        }
        // If this is an end-of-day reading, complete the latest run.
        else if (request.ReadingType == OdometerReadingType.EndOfDay)
        {
            // Retrieve the most recent run for this user and vehicle.
            var run = await _vehicleDayRunRepository.GetLatestByUserAndVehicleAsync(
                userId,
                request.VehicleAssetId,
                cancellationToken);

            if (run is not null)
            {
                // Set end-of-day values.
                run.EndOdometerKm = request.ReadingKm;
                run.FinishedAtUtc = DateTime.UtcNow;

                // Update notes only if new notes were provided.
                run.Notes = string.IsNullOrWhiteSpace(request.Notes) ? run.Notes : request.Notes;

                await _vehicleDayRunRepository.UpdateAsync(run, cancellationToken);
            }
        }

        // Return success response.
        return Ok(new OdometerReadingResponse
        {
            Success = true,
            Message = "Odometer reading recorded successfully."
        });
    }
}