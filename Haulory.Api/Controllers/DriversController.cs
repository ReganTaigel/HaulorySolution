using Haulory.Api.Drivers;
using Haulory.Api.Extensions;
using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Drivers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize]
public sealed class DriversController : ControllerBase
{
    private readonly IDriverRepository _driverRepository;
    private readonly CreateDriverHandler _createDriverHandler;
    private readonly IDriverInductionRepository _driverInductions;
    private readonly IInductionEvidenceFileStorage _fileStorage;
    private readonly DriverRequestValidator _validator;
    private readonly DriverWorkflowService _workflowService;

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
        _validator = new DriverRequestValidator();
        _workflowService = new DriverWorkflowService(driverRepository);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DriverDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DriverDto>>> GetAll()
    {
        var ownerUserId = User.GetOwnerUserId();
        var drivers = await _driverRepository.GetAllByOwnerUserIdAsync(ownerUserId);

        return Ok(drivers
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => x.ToDto())
            .ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(DriverDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DriverDto>> GetById(Guid id)
    {
        var ownerUserId = User.GetOwnerUserId();
        var driver = await _driverRepository.GetByIdForOwnerAsync(ownerUserId, id);

        if (driver is null)
            return NotFound();

        return Ok(driver.ToDto());
    }

    [HttpPost]
    [ProducesResponseType(typeof(DriverDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<DriverDto>> Create([FromBody] CreateDriverRequest request)
    {
        _validator.ValidateCreate(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

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
                Line2: request.Line2,
                Suburb: request.Suburb,
                City: request.City,
                Region: request.Region,
                Postcode: request.Postcode,
                Country: request.Country,
                EmergencyFirstName: request.EmergencyContact?.FirstName ?? string.Empty,
                EmergencyLastName: request.EmergencyContact?.LastName ?? string.Empty,
                EmergencyRelationship: request.EmergencyContact?.Relationship ?? string.Empty,
                EmergencyEmail: request.EmergencyContact?.Email ?? string.Empty,
                EmergencyPhoneNumber: request.EmergencyContact?.PhoneNumber ?? string.Empty,
                EmergencySecondaryPhoneNumber: request.EmergencyContact?.SecondaryPhoneNumber,
                CreateLoginAccount: request.CreateLoginAccount,
                Password: request.Password
            ));

        if (created is null)
            return BadRequest("Unable to create driver.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(DriverDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DriverDto>> Update(Guid id, [FromBody] UpdateDriverRequest request)
    {
        _validator.ValidateUpdate(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();
        var driver = await _workflowService.UpdateAsync(ownerUserId, id, request);
        if (driver is null)
            return NotFound();

        return Ok(driver.ToDto());
    }

    [HttpPost("{driverId:guid}/inductions/{workSiteId:guid}/{requirementId:guid}/evidence")]
    [RequestSizeLimit(25_000_000)]
    public async Task<IActionResult> UploadInductionEvidence(
        Guid driverId,
        Guid workSiteId,
        Guid requirementId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        var ownerId = User.GetOwnerUserId();

        if (file == null || file.Length == 0)
            return BadRequest("A file is required.");

        var entity = await _driverInductions.GetAsync(ownerId, driverId, workSiteId, requirementId);
        if (entity == null)
            return NotFound("Driver induction not found.");

        if (!string.IsNullOrWhiteSpace(entity.EvidenceFilePath))
            await _fileStorage.DeleteAsync(entity.EvidenceFilePath, cancellationToken);

        await using var stream = file.OpenReadStream();
        var saved = await _fileStorage.SaveAsync(stream, file.FileName, file.ContentType, cancellationToken);
        entity.SetEvidence(file.FileName, saved.contentType, saved.storedRelativePath);

        await _driverInductions.UpdateAsync(ownerId, driverId, entity);

        return Ok(new
        {
            fileName = entity.EvidenceFileName,
            contentType = entity.EvidenceContentType,
            uploadedOnUtc = entity.EvidenceUploadedOnUtc,
            url = BuildAbsoluteFileUrl(entity.EvidenceFilePath)
        });
    }

    [HttpDelete("{driverId:guid}/inductions/{workSiteId:guid}/{requirementId:guid}/evidence")]
    public async Task<IActionResult> DeleteInductionEvidence(
        Guid driverId,
        Guid workSiteId,
        Guid requirementId,
        CancellationToken cancellationToken)
    {
        var ownerId = User.GetOwnerUserId();

        var entity = await _driverInductions.GetAsync(ownerId, driverId, workSiteId, requirementId);
        if (entity == null)
            return NotFound("Driver induction not found.");

        await _fileStorage.DeleteAsync(entity.EvidenceFilePath, cancellationToken);
        entity.ClearEvidence();
        await _driverInductions.UpdateAsync(ownerId, driverId, entity);

        return NoContent();
    }

    private string? BuildAbsoluteFileUrl(string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath))
            return null;

        return $"{Request.Scheme}://{Request.Host}{relativePath}";
    }
}
