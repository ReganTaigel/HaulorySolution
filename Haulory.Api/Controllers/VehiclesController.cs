using Haulory.Api.Extensions;
using Haulory.Api.Vehicles;
using Haulory.Application.Features.Vehicles;
using Haulory.Application.Features.Vehicles.CreateVehicleSet;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Contracts.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

// Marks this as an API controller with automatic binding and validation.
[ApiController]

// Supports both routes: api/vehicles and api/vehicleassets
[Route("api/vehicles")]
[Route("api/vehicleassets")]

// Requires authentication for all endpoints.
[Authorize]
public sealed class VehiclesController : ControllerBase
{
    // Repository for vehicle asset data access.
    private readonly IVehicleAssetRepository _vehicleRepository;

    // Handler responsible for creating vehicle sets.
    private readonly CreateVehicleHandler _createVehicleHandler;

    // Validator for vehicle set creation requests.
    private readonly VehicleSetRequestValidator _validator;

    // Mapper used to convert request data into domain entities.
    private readonly VehicleSetMapper _mapper;

    // Service responsible for deleting vehicles.
    private readonly DeleteVehicle _deleteVehicle;

    // Constructor injection of dependencies.
    public VehiclesController(
        IVehicleAssetRepository vehicleRepository,
        CreateVehicleHandler createVehicleHandler,
        DeleteVehicle deleteVehicle)
    {
        _vehicleRepository = vehicleRepository;
        _createVehicleHandler = createVehicleHandler;
        _deleteVehicle = deleteVehicle;

        // Validator and mapper are instantiated locally.
        _validator = new VehicleSetRequestValidator();
        _mapper = new VehicleSetMapper();
    }

    // Retrieves all vehicles for the current owner.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
    {
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve vehicles scoped to owner.
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

        // Sort and map to DTOs.
        return Ok(vehicles
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList());
    }

    // Retrieves a specific vehicle by ID if it belongs to the current owner.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(VehicleDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleDto>> GetById(Guid id)
    {
        var ownerUserId = User.GetOwnerUserId();

        var vehicle = await _vehicleRepository.GetByIdAsync(id);

        // Ensure vehicle exists and belongs to the owner.
        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound();

        return Ok(vehicle.ToDto());
    }

    // Retrieves only trailer-type vehicle assets.
    [HttpGet("trailers")]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetTrailers()
    {
        var ownerUserId = User.GetOwnerUserId();

        var trailers = await _vehicleRepository.GetTrailerAssetsByOwnerAsync(ownerUserId);

        return Ok(trailers
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList());
    }

    // Retrieves vehicles by owner ID (with security check).
    [HttpGet("owner/{ownerUserId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetByOwner(Guid ownerUserId)
    {
        // Ensure the requested owner matches the authenticated owner.
        var authenticatedOwnerUserId = User.GetOwnerUserId();
        if (ownerUserId != authenticatedOwnerUserId)
            return Forbid();

        var vehicles = await _vehicleRepository.GetByOwnerAsync(authenticatedOwnerUserId);

        return Ok(vehicles
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList());
    }

    // Creates a new vehicle set (multiple related vehicle assets grouped together).
    [HttpPost("sets")]
    [ProducesResponseType(typeof(CreateVehicleSetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateVehicleSetResponse>> CreateSet(
        [FromBody] CreateVehicleSetRequest request,
        CancellationToken cancellationToken)
    {
        // Validate request.
        var validationError = _validator.Validate(request);
        if (validationError != null)
            return BadRequest(validationError);

        var ownerUserId = User.GetOwnerUserId();

        // Generate a new vehicle set identifier.
        var setId = Guid.NewGuid();

        List<Haulory.Domain.Entities.VehicleAsset> assets;

        try
        {
            // Map request into domain vehicle assets.
            assets = _mapper.MapAssets(ownerUserId, setId, request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }

        // Persist the vehicle set via handler.
        var result = await _createVehicleHandler.HandleAsync(
            new CreateVehicleCommand
            {
                OwnerUserId = ownerUserId,
                Assets = assets
            },
            cancellationToken);

        // Handle failure cases.
        if (!result.Success)
            return BadRequest(result.Message);

        if (!result.VehicleSetId.HasValue)
            return BadRequest("Vehicle set id was not returned.");

        // Return created vehicle set details.
        return StatusCode(StatusCodes.Status201Created, new CreateVehicleSetResponse
        {
            Message = result.Message,
            VehicleSetId = result.VehicleSetId.Value,
            AssetsCreated = result.AssetsCreated,
            Assets = assets.Select(x => x.ToDto()).ToList()
        });
    }

    // Deletes a vehicle asset.
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var ownerUserId = User.GetOwnerUserId();

        // Ensure vehicle exists and belongs to the owner.
        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound();

        // Perform deletion via domain/service layer.
        var deleted = await _deleteVehicle.DeleteAsync(id, cancellationToken);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}