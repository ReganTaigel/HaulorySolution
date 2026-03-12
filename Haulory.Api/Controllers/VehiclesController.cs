using Haulory.Api.Contracts.Vehicles;
using Haulory.Api.Extensions;
using Haulory.Application.Features.Vehicles.CreateVehicleSet;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
[Route("api/vehicleassets")] // compatibility route for mobile app
[Authorize]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleAssetRepository _vehicleRepository;
    private readonly CreateVehicleHandler _createVehicleHandler;

    public VehiclesController(
        IVehicleAssetRepository vehicleRepository,
        CreateVehicleHandler createVehicleHandler)
    {
        _vehicleRepository = vehicleRepository;
        _createVehicleHandler = createVehicleHandler;
    }

    // GET: api/vehicles
    // GET: api/vehicleassets
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
    {
        System.Diagnostics.Debug.WriteLine("[VehiclesController] GetAll called");

        var ownerUserId = User.GetOwnerUserId();
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

        var result = vehicles
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList();

        return Ok(result);
    }

    // GET: api/vehicles/{id}
    // GET: api/vehicleassets/{id}
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id)
    {
        var ownerUserId = User.GetOwnerUserId();

        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound();

        return Ok(vehicle.ToDto());
    }

    // GET: api/vehicles/trailers
    // GET: api/vehicleassets/trailers
    [HttpGet("trailers")]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetTrailers()
    {
        System.Diagnostics.Debug.WriteLine("[VehiclesController] GetTrailers called");

        var ownerUserId = User.GetOwnerUserId();
        var trailers = await _vehicleRepository.GetTrailerAssetsByOwnerAsync(ownerUserId);

        var result = trailers
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList();

        return Ok(result);
    }

    // GET: api/vehicles/owner/{ownerUserId}
    // GET: api/vehicleassets/owner/{ownerUserId}
    // compatibility endpoint - still uses authenticated owner's vehicles
    [HttpGet("owner/{ownerUserId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetByOwner(Guid ownerUserId)
    {
        var authenticatedOwnerUserId = User.GetOwnerUserId();

        // prevent users from querying a different owner's vehicles
        if (ownerUserId != authenticatedOwnerUserId)
            return Forbid();

        var vehicles = await _vehicleRepository.GetByOwnerAsync(authenticatedOwnerUserId);

        var result = vehicles
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList();

        return Ok(result);
    }

    // POST: api/vehicles/sets
    // POST: api/vehicleassets/sets
    [HttpPost("sets")]
    [ProducesResponseType(typeof(CreateVehicleSetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateVehicleSetResponse>> CreateSet(
        [FromBody] CreateVehicleSetRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Units == null || request.Units.Count == 0)
            return BadRequest("At least one unit is required.");

        var ownerUserId = User.GetOwnerUserId();
        var setId = Guid.NewGuid();

        var assets = new List<VehicleAsset>();

        foreach (var unit in request.Units.OrderBy(x => x.UnitNumber))
        {
            if (unit.UnitNumber < 1 || unit.UnitNumber > 3)
                return BadRequest($"Invalid unit number: {unit.UnitNumber}");

            if (string.IsNullOrWhiteSpace(unit.Kind))
                return BadRequest($"Kind is required for unit {unit.UnitNumber}.");

            if (string.IsNullOrWhiteSpace(unit.VehicleType))
                return BadRequest($"Vehicle type is required for unit {unit.UnitNumber}.");

            if (string.IsNullOrWhiteSpace(unit.Rego))
                return BadRequest($"Rego is required for unit {unit.UnitNumber}.");

            if (string.IsNullOrWhiteSpace(unit.Make))
                return BadRequest($"Make is required for unit {unit.UnitNumber}.");

            if (string.IsNullOrWhiteSpace(unit.Model))
                return BadRequest($"Model is required for unit {unit.UnitNumber}.");

            if (unit.Year <= 0)
                return BadRequest($"Year is required for unit {unit.UnitNumber}.");

            if (string.IsNullOrWhiteSpace(unit.CertificateType))
                return BadRequest($"Certificate type is required for unit {unit.UnitNumber}.");

            if (!Enum.TryParse<AssetKind>(unit.Kind, true, out var kind))
                return BadRequest($"Invalid kind for unit {unit.UnitNumber}: {unit.Kind}");

            if (!Enum.TryParse<VehicleType>(unit.VehicleType, true, out var vehicleType))
                return BadRequest($"Invalid vehicle type for unit {unit.UnitNumber}: {unit.VehicleType}");

            if (!Enum.TryParse<ComplianceCertificateType>(unit.CertificateType, true, out var certificateType))
                return BadRequest($"Invalid certificate type for unit {unit.UnitNumber}: {unit.CertificateType}");

            FuelType? fuelType = null;
            if (!string.IsNullOrWhiteSpace(unit.FuelType))
            {
                if (!Enum.TryParse<FuelType>(unit.FuelType, true, out var parsedFuel))
                    return BadRequest($"Invalid fuel type for unit {unit.UnitNumber}: {unit.FuelType}");

                fuelType = parsedFuel;
            }

            VehicleConfiguration? configuration = null;
            if (!string.IsNullOrWhiteSpace(unit.Configuration))
            {
                if (!Enum.TryParse<VehicleConfiguration>(unit.Configuration, true, out var parsedConfig))
                    return BadRequest($"Invalid configuration for unit {unit.UnitNumber}: {unit.Configuration}");

                configuration = parsedConfig;
            }

            var asset = new VehicleAsset
            {
                OwnerUserId = ownerUserId,
                VehicleSetId = setId,
                UnitNumber = unit.UnitNumber,
                Kind = kind,
                VehicleType = vehicleType,
                FuelType = fuelType,
                Configuration = configuration,

                Rego = unit.Rego.Trim().ToUpperInvariant(),
                RegoExpiry = unit.RegoExpiry,

                Make = unit.Make.Trim(),
                Model = unit.Model.Trim(),
                Year = unit.Year,

                CertificateType = certificateType,
                CertificateExpiry = unit.CertificateExpiry,

                OdometerKm = unit.OdometerKm,

                RucPurchasedDate = unit.RucPurchasedDate,
                RucDistancePurchasedKm = unit.RucDistancePurchasedKm,
                RucLicenceStartKm = unit.RucLicenceStartKm,
                RucLicenceEndKm = unit.RucLicenceEndKm,
                RucNextDueOdometerKm = unit.RucLicenceEndKm
            };

            assets.Add(asset);
        }

        var result = await _createVehicleHandler.HandleAsync(
            new CreateVehicleCommand
            {
                OwnerUserId = ownerUserId,
                Assets = assets
            },
            cancellationToken);

        if (!result.Success)
            return BadRequest(result.Message);

        if (!result.VehicleSetId.HasValue)
            return BadRequest("Vehicle set id was not returned.");

        var response = new CreateVehicleSetResponse
        {
            Message = result.Message,
            VehicleSetId = result.VehicleSetId.Value,
            AssetsCreated = result.AssetsCreated,
            Assets = assets.Select(x => x.ToDto()).ToList()
        };

        return StatusCode(StatusCodes.Status201Created, response);
    }
}