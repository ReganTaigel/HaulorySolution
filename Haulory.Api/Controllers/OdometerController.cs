using Haulory.Api.Extensions;
using Haulory.Contracts.Vehicles;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Haulory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/odometer")]
[Authorize]
public sealed class OdometerController : ControllerBase
{
    private readonly IVehicleAssetRepository _vehicleRepository;
    private readonly IVehicleDayRunRepository _vehicleDayRunRepository;
    private readonly HauloryDbContext _context;

    public OdometerController(
        IVehicleAssetRepository vehicleRepository,
        IVehicleDayRunRepository vehicleDayRunRepository,
        HauloryDbContext context)
    {
        _vehicleRepository = vehicleRepository;
        _vehicleDayRunRepository = vehicleDayRunRepository;
        _context = context;
    }

    [HttpGet("assets")]
    public async Task<IActionResult> GetAssets(CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine("[OdometerController] GET assets hit");

        var ownerUserId = User.GetOwnerUserId();
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

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

    [HttpPost("readings")]
    public async Task<IActionResult> RecordReading(
        [FromBody] OdometerReadingRequest request,
        CancellationToken cancellationToken)
    {
        System.Diagnostics.Debug.WriteLine("[OdometerController] POST readings hit");

        if (request is null)
            return BadRequest("Request is required.");

        if (request.VehicleAssetId == Guid.Empty)
            return BadRequest("VehicleAssetId is required.");

        if (request.ReadingKm <= 0)
            return BadRequest("ReadingKm must be greater than zero.");

        var ownerUserId = User.GetOwnerUserId();
        var userId = User.GetUserId();

        var vehicle = await _vehicleRepository.GetByIdAsync(request.VehicleAssetId);
        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound("Vehicle not found.");

        var reading = new OdometerReading(
            request.VehicleAssetId,
            vehicle.UnitNumber,
            request.ReadingKm,
            request.ReadingType,
            request.DriverId,
            request.RecordedByUserId ?? userId,
            request.Notes);

        _context.OdometerReadings.Add(reading);
        await _context.SaveChangesAsync(cancellationToken);

        if (request.UpdateCurrentOdometer)
        {
            vehicle.OdometerKm = request.ReadingKm;
            await _vehicleRepository.UpdateAsync(vehicle, cancellationToken);
        }

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
        else if (request.ReadingType == OdometerReadingType.EndOfDay)
        {
            var run = await _vehicleDayRunRepository.GetLatestByUserAndVehicleAsync(
                userId,
                request.VehicleAssetId,
                cancellationToken);

            if (run is not null)
            {
                run.EndOdometerKm = request.ReadingKm;
                run.FinishedAtUtc = DateTime.UtcNow;
                run.Notes = string.IsNullOrWhiteSpace(request.Notes) ? run.Notes : request.Notes;

                await _vehicleDayRunRepository.UpdateAsync(run, cancellationToken);
            }
        }
        return Ok(new OdometerReadingResponse
        {
            Success = true,
            Message = "Odometer reading recorded successfully."
        });

    }
}
