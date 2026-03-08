using System.Security.Claims;
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

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
    {
        var ownerUserId = GetOwnerUserId();

        var allVehicles = await _vehicleRepository.GetAllAsync();

        var result = allVehicles
            .Where(x => x.OwnerUserId == ownerUserId)
            .OrderBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList();

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id)
    {
        var ownerUserId = GetOwnerUserId();

        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound();

        return Ok(vehicle.ToDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create([FromBody] CreateVehicleRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Rego))
            ModelState.AddModelError(nameof(request.Rego), "Rego is required.");

        if (string.IsNullOrWhiteSpace(request.Make))
            ModelState.AddModelError(nameof(request.Make), "Make is required.");

        if (string.IsNullOrWhiteSpace(request.Model))
            ModelState.AddModelError(nameof(request.Model), "Model is required.");

        if (request.Year <= 0)
            ModelState.AddModelError(nameof(request.Year), "Year is required.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);
        var ownerUserId = GetOwnerUserId();

        var setId = Guid.NewGuid();

        var asset = new VehicleAsset
        {
            OwnerUserId = ownerUserId,
            VehicleSetId = setId,
            Kind = AssetKind.PowerUnit,
            Rego = request.Rego.Trim().ToUpperInvariant(),
            Make = request.Make.Trim(),
            Model = request.Model.Trim(),
            Year = request.Year,
            UnitNumber = request.UnitNumber ?? 1,
            OdometerKm = request.OdometerKm
        };

        if (!string.IsNullOrWhiteSpace(request.VehicleType) &&
            Enum.TryParse<VehicleType>(request.VehicleType, true, out var vehicleType))
        {
            asset.VehicleType = vehicleType;
        }

        if (!string.IsNullOrWhiteSpace(request.FuelType) &&
            Enum.TryParse<FuelType>(request.FuelType, true, out var fuelType))
        {
            asset.FuelType = fuelType;
        }

        if (!string.IsNullOrWhiteSpace(request.Configuration) &&
            Enum.TryParse<VehicleConfiguration>(request.Configuration, true, out var configuration))
        {
            asset.Configuration = configuration;
        }

        var result = await _createVehicleHandler.HandleAsync(
            new CreateVehicleCommand
            {
                OwnerUserId = ownerUserId,
                Assets = new List<VehicleAsset> { asset }
            },
            cancellationToken);

        if (!result.Success)
            return BadRequest(result.Message);

        return CreatedAtAction(
            nameof(GetById),
            new { id = asset.Id },
            new
            {
                message = result.Message,
                vehicleSetId = result.VehicleSetId,
                assetsCreated = result.AssetsCreated,
                asset = asset.ToDto()
            });
    }

    private Guid GetOwnerUserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId");

        if (!Guid.TryParse(value, out var ownerUserId))
            throw new UnauthorizedAccessException("Authenticated user id is missing or invalid.");

        return ownerUserId;
    }

    [HttpGet("trailers")]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetTrailers()
    {
        var ownerUserId = GetOwnerUserId();

        var allVehicles = await _vehicleRepository.GetAllAsync();

        var trailers = allVehicles
            .Where(x => x.OwnerUserId == ownerUserId && x.Kind == AssetKind.Trailer)
            .OrderBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList();

        return Ok(trailers);
    }
}