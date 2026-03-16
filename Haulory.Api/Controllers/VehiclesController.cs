using Haulory.Api.Extensions;
using Haulory.Api.Vehicles;
using Haulory.Application.Features.Vehicles.CreateVehicleSet;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Contracts.Vehicles;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/vehicles")]
[Route("api/vehicleassets")]
[Authorize]
public sealed class VehiclesController : ControllerBase
{
    private readonly IVehicleAssetRepository _vehicleRepository;
    private readonly CreateVehicleHandler _createVehicleHandler;
    private readonly VehicleSetRequestValidator _validator;
    private readonly VehicleSetMapper _mapper;

    public VehiclesController(
        IVehicleAssetRepository vehicleRepository,
        CreateVehicleHandler createVehicleHandler)
    {
        _vehicleRepository = vehicleRepository;
        _createVehicleHandler = createVehicleHandler;
        _validator = new VehicleSetRequestValidator();
        _mapper = new VehicleSetMapper();
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetAll()
    {
        var ownerUserId = User.GetOwnerUserId();
        var vehicles = await _vehicleRepository.GetByOwnerAsync(ownerUserId);

        return Ok(vehicles
            .OrderBy(x => x.VehicleSetId)
            .ThenBy(x => x.UnitNumber)
            .ThenBy(x => x.Rego)
            .Select(x => x.ToDto())
            .ToList());
    }

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

    [HttpGet("owner/{ownerUserId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<VehicleDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VehicleDto>>> GetByOwner(Guid ownerUserId)
    {
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

    [HttpPost("sets")]
    [ProducesResponseType(typeof(CreateVehicleSetResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<CreateVehicleSetResponse>> CreateSet(
        [FromBody] CreateVehicleSetRequest request,
        CancellationToken cancellationToken)
    {
        var validationError = _validator.Validate(request);
        if (validationError != null)
            return BadRequest(validationError);

        var ownerUserId = User.GetOwnerUserId();
        var setId = Guid.NewGuid();

        List<Haulory.Domain.Entities.VehicleAsset> assets;
        try
        {
            assets = _mapper.MapAssets(ownerUserId, setId, request);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
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

        return StatusCode(StatusCodes.Status201Created, new CreateVehicleSetResponse
        {
            Message = result.Message,
            VehicleSetId = result.VehicleSetId.Value,
            AssetsCreated = result.AssetsCreated,
            Assets = assets.Select(x => x.ToDto()).ToList()
        });
    }
}
