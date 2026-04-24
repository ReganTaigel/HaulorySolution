using HaulitCore.Api.Extensions;
using HaulitCore.Api.Vehicles;
using HaulitCore.Application.Features.Vehicles;
using HaulitCore.Application.Features.Vehicles.CreateVehicleSet;
using HaulitCore.Application.Features.Vehicles.UpdateVehicleSet;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Contracts.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaulitCore.Api.Controllers;

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

    // Service responsible for updating vehicle sets.
    private readonly UpdateVehicleSetHandler _updateVehicleSetHandler;

    // Constructor injection of dependencies.
    public VehiclesController(
        IVehicleAssetRepository vehicleRepository,
        CreateVehicleHandler createVehicleHandler,
        DeleteVehicle deleteVehicle,
        UpdateVehicleSetHandler updateVehicleSetHandler)
    {
        _vehicleRepository = vehicleRepository;
        _createVehicleHandler = createVehicleHandler;
        _deleteVehicle = deleteVehicle;
        _updateVehicleSetHandler = updateVehicleSetHandler;

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

        try
        {
            var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

            return Ok(vehicles
                .OrderBy(x => x.VehicleSetId)
                .ThenBy(x => x.UnitNumber)
                .ThenBy(x => x.Rego)
                .Select(x => x.ToDto())
                .ToList());
        }
        catch (Exception ex)
        {
            Console.WriteLine(" VEHICLES ERROR:");
            Console.WriteLine(ex.ToString());

            throw; // rethrow so Azure logs it
        }
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

        try
        {
            var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

            return Ok(vehicles
                .OrderBy(x => x.VehicleSetId)
                .ThenBy(x => x.UnitNumber)
                .ThenBy(x => x.Rego)
                .Select(x => x.ToDto())
                .ToList());
        }
        catch (Exception ex)
        {
            return StatusCode(500, ex.ToString()); // TEMP
        }
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

        List<HaulitCore.Domain.Entities.VehicleAsset> assets;

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

    // Returns the full vehicle set for a given vehicle id.
    // Needed so the mobile edit screen can load unit 1/2/3 together.
    [HttpGet("{id:guid}/set")]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetSetByVehicleId(Guid id, CancellationToken cancellationToken)
    {
        var ownerUserId = User.GetOwnerUserId();

        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound();

        var vehiclesInSet = await _vehicleRepository.GetByVehicleSetIdAsync(vehicle.VehicleSetId, cancellationToken);

        return Ok(vehiclesInSet
            .OrderBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList());
    }

    // Full set update endpoint.
    // This is the endpoint your mobile UpdateVehicleSetAsync should call.
    [HttpPut("{id:guid}/set")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateSet(
    Guid id,
    [FromBody] UpdateVehicleSetRequest request,
    CancellationToken cancellationToken)
    {
        var ownerUserId = User.GetOwnerUserId();

        var vehicle = await _vehicleRepository.GetByIdAsync(id);
        if (vehicle is null || vehicle.OwnerUserId != ownerUserId)
            return NotFound();

        var result = await _updateVehicleSetHandler.HandleAsync(
            new UpdateVehicleSetCommand
            {
                VehicleAssetId = id,
                OwnerUserId = ownerUserId,
                Request = request
            },
            cancellationToken);

        if (!result.Success)
            return BadRequest(result.Message);

        return NoContent();
    }
}