using HaulitCore.Api.Drivers;
using HaulitCore.Api.Extensions;
using HaulitCore.Application.Features.Drivers;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Contracts.Drivers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace HaulitCore.Api.Controllers;

// Marks this as an API controller with automatic model binding and validation.
[ApiController]

// Base route: api/drivers
[Route("api/drivers")]

// Requires authentication for all endpoints.
[Authorize]
public sealed class DriversController : ControllerBase
{
    // Repository for accessing driver data.
    private readonly IDriverRepository _driverRepository;

    // Handler responsible for creating drivers.
    private readonly CreateDriverHandler _createDriverHandler;

    // Repository for driver induction records.
    private readonly IDriverInductionRepository _driverInductions;

    // Service responsible for storing and retrieving uploaded files.
    private readonly IInductionEvidenceFileStorage _fileStorage;

    // Validator for incoming driver requests.
    private readonly DriverRequestValidator _validator;

    // Service handling driver update workflows and business logic.
    private readonly DriverWorkflowService _workflowService;

    // Constructor injection of dependencies.
    public DriversController(
        IDriverRepository driverRepository,
        CreateDriverHandler createDriverHandler,
        IDriverInductionRepository driverInductions,
        IInductionEvidenceFileStorage fileStorage)
    {
        _driverRepository = driverRepository;
        _createDriverHandler = createDriverHandler;
        _driverInductions = driverInductions;
        _fileStorage = fileStorage;

        // Validator and workflow service are instantiated locally.
        _validator = new DriverRequestValidator();
        _workflowService = new DriverWorkflowService(driverRepository);
    }

    // Returns all drivers belonging to the current owner.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DriverDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DriverDto>>> GetAll()
    {
        // Extract owner ID from authenticated user claims.
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve all drivers for this owner.
        var drivers = await _driverRepository.GetAllByOwnerUserIdAsync(ownerUserId);

        // Sort drivers by name and map to DTOs.
        return Ok(drivers
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => x.ToDto())
            .ToList());
    }

    // Returns a specific driver by ID if it belongs to the current owner.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DriverDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DriverDto>> GetById(Guid id)
    {
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve driver scoped to owner.
        var driver = await _driverRepository.GetByIdForOwnerAsync(ownerUserId, id);

        if (driver is null)
            return NotFound();

        return Ok(driver.ToDto());
    }

    // Creates a new driver record.
    [HttpPost]
    [ProducesResponseType(typeof(DriverDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DriverDto>> Create([FromBody] CreateDriverRequest request)
    {
        // Validate incoming request.
        _validator.ValidateCreate(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        // Create driver using application handler (encapsulates business logic).
        var created = await _createDriverHandler.HandleAsync(
            new CreateDriverCommand(
                OwnerUserId: ownerUserId,
                FirstName: request.FirstName.Trim(),
                LastName: request.LastName.Trim(),
                Email: request.Email.Trim().ToLowerInvariant(),

                LicenceNumber: request.LicenceNumber,
                PhoneNumber: request.PhoneNumber,
                DateOfBirthUtc: request.DateOfBirthUtc,
                LicenceExpiresOnUtc: request.LicenceExpiresOnUtc,
                LicenceVersion: request.LicenceVersion,
                LicenceClassOrEndorsements: request.LicenceClassOrEndorsements,
                LicenceIssuedOnUtc: request.LicenceIssuedOnUtc,
                LicenceConditionsNotes: request.LicenceConditionsNotes,

                Line1: request.Line1,
                Suburb: request.Suburb,
                City: request.City,
                Region: request.Region,
                Postcode: request.Postcode,
                Country: request.Country,

                // Emergency contact fields (optional).
                EmergencyFirstName: request.EmergencyContact?.FirstName ?? string.Empty,
                EmergencyLastName: request.EmergencyContact?.LastName ?? string.Empty,
                EmergencyRelationship: request.EmergencyContact?.Relationship ?? string.Empty,
                EmergencyEmail: request.EmergencyContact?.Email ?? string.Empty,
                EmergencyPhoneNumber: request.EmergencyContact?.PhoneNumber ?? string.Empty,
                EmergencySecondaryPhoneNumber: request.EmergencyContact?.SecondaryPhoneNumber,

                // Optionally create a login account for the driver.
                CreateLoginAccount: request.CreateLoginAccount,
                Password: request.Password
            ));

        if (created is null)
            return BadRequest("Unable to create driver.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    // Updates an existing driver record.
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DriverDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DriverDto>> Update(Guid id, [FromBody] UpdateDriverRequest request)
    {
        // Validate incoming request.
        _validator.ValidateUpdate(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        // Perform update via workflow service to enforce business rules.
        var driver = await _workflowService.UpdateAsync(ownerUserId, id, request);

        if (driver is null)
            return NotFound();

        return Ok(driver.ToDto());
    }

    // Uploads evidence (e.g., document/photo) for a specific driver induction requirement.
    [HttpPost("{driverId:guid}/inductions/{workSiteId:guid}/{requirementId:guid}/evidence")]
    [RequestSizeLimit(25_000_000)] // Limit file size to ~25MB
    public async Task<IActionResult> UploadInductionEvidence(
        Guid driverId,
        Guid workSiteId,
        Guid requirementId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var ownerId = User.GetOwnerUserId();

        // Ensure a file was provided.
        if (file == null || file.Length == 0)
            return BadRequest("A file is required.");

        // Retrieve the induction entity.
        var entity = await _driverInductions.GetAsync(ownerId, driverId, workSiteId, requirementId);
        if (entity == null)
            return NotFound("Driver induction not found.");

        // If an existing file exists, delete it before replacing.
        if (!string.IsNullOrWhiteSpace(entity.EvidenceFilePath))
            await _fileStorage.DeleteAsync(entity.EvidenceFilePath, cancellationToken);

        // Save the new file.
        await using var stream = file.OpenReadStream();
        var saved = await _fileStorage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);

        // Update entity with file metadata.
        entity.SetEvidence(file.FileName, saved.contentType, saved.storedRelativePath);

        // Persist the updated entity.
        await _driverInductions.UpdateAsync(ownerId, driverId, entity);

        // Return file details including a public URL.
        return Ok(new
        {
            fileName = entity.EvidenceFileName,
            contentType = entity.EvidenceContentType,
            uploadedOnUtc = entity.EvidenceUploadedOnUtc,
            url = BuildAbsoluteFileUrl(entity.EvidenceFilePath)
        });
    }

    // Deletes evidence associated with a driver induction requirement.
    [HttpDelete("{driverId:guid}/inductions/{workSiteId:guid}/{requirementId:guid}/evidence")]
    public async Task<IActionResult> DeleteInductionEvidence(
        Guid driverId,
        Guid workSiteId,
        Guid requirementId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.GetOwnerUserId();

        // Retrieve the induction entity.
        var entity = await _driverInductions.GetAsync(ownerId, driverId, workSiteId, requirementId);
        if (entity == null)
            return NotFound("Driver induction not found.");

        // Delete the stored file.
        await _fileStorage.DeleteAsync(entity.EvidenceFilePath, cancellationToken);

        // Clear evidence metadata from the entity.
        entity.ClearEvidence();

        // Persist changes.
        await _driverInductions.UpdateAsync(ownerId, driverId, entity);

        return NoContent();
    }

    // Builds a full absolute URL from a stored relative file path.
    private string? BuildAbsoluteFileUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        return $"{Request.Scheme}://{Request.Host}{relativePath}";
    }
}