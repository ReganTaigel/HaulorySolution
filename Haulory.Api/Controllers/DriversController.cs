using Haulory.Api.Contracts.Drivers;
using Haulory.Api.Extensions;
using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Domain.Entities;
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
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DriverDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DriverDto>>> GetAll()
    {
        var ownerUserId = User.GetOwnerUserId();

        var drivers = await _driverRepository.GetAllByOwnerUserIdAsync(ownerUserId);

        var result = drivers
            .OrderBy(x => x.FirstName)
            .ThenBy(x => x.LastName)
            .Select(x => x.ToDto())
            .ToList();

        return Ok(result);
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
        if (string.IsNullOrWhiteSpace(request.FirstName))
            ModelState.AddModelError(nameof(request.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            ModelState.AddModelError(nameof(request.LastName), "Last name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            ModelState.AddModelError(nameof(request.Email), "Email is required.");
        if (request.EmergencyContact is null)
        {
            ModelState.AddModelError(nameof(request.EmergencyContact), "Emergency contact is required.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(request.EmergencyContact.FirstName))
                ModelState.AddModelError(nameof(request.EmergencyContact.FirstName), "Emergency contact first name is required.");

            if (string.IsNullOrWhiteSpace(request.EmergencyContact.LastName))
                ModelState.AddModelError(nameof(request.EmergencyContact.LastName), "Emergency contact last name is required.");

            if (string.IsNullOrWhiteSpace(request.EmergencyContact.Relationship))
                ModelState.AddModelError(nameof(request.EmergencyContact.Relationship), "Emergency contact relationship is required.");

            if (string.IsNullOrWhiteSpace(request.EmergencyContact.Email))
                ModelState.AddModelError(nameof(request.EmergencyContact.Email), "Emergency contact email is required.");

            if (string.IsNullOrWhiteSpace(request.EmergencyContact.PhoneNumber))
                ModelState.AddModelError(nameof(request.EmergencyContact.PhoneNumber), "Emergency contact phone number is required.");
        }

        if (request.CreateLoginAccount && string.IsNullOrWhiteSpace(request.Password))
            ModelState.AddModelError(nameof(request.Password), "Password is required when creating a login account.");

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
        if (string.IsNullOrWhiteSpace(request.FirstName))
            ModelState.AddModelError(nameof(request.FirstName), "First name is required.");

        if (string.IsNullOrWhiteSpace(request.LastName))
            ModelState.AddModelError(nameof(request.LastName), "Last name is required.");

        if (string.IsNullOrWhiteSpace(request.Email))
            ModelState.AddModelError(nameof(request.Email), "Email is required.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        var driver = await _driverRepository.GetByIdForOwnerAsync(ownerUserId, id);
        if (driver is null)
            return NotFound();

        driver.UpdateIdentity(request.FirstName, request.LastName, request.Email);
        driver.UpdatePhone(request.PhoneNumber);
        driver.UpdateDateOfBirthUtc(request.DateOfBirthUtc);

        driver.UpdateLicenceNumber(request.LicenceNumber);
        driver.UpdateLicenceVersion(request.LicenceVersion);
        driver.UpdateLicenceClassOrEndorsements(request.LicenceClassOrEndorsements);
        driver.UpdateLicenceIssuedOnUtc(request.LicenceIssuedOnUtc);
        driver.UpdateLicenceExpiryUtc(request.LicenceExpiresOnUtc);
        driver.UpdateLicenceConditionsNotes(request.LicenceConditionsNotes);

        driver.UpdateAddress(
            request.Line1,
            request.Line2,
            request.Suburb,
            request.City,
            request.Region,
            request.Postcode,
            request.Country
        );

        driver.UpdateEmergencyContact(new EmergencyContact(
            request.EmergencyContact?.FirstName ?? string.Empty,
            request.EmergencyContact?.LastName ?? string.Empty,
            request.EmergencyContact?.Relationship ?? string.Empty,
            request.EmergencyContact?.Email ?? string.Empty,
            request.EmergencyContact?.PhoneNumber ?? string.Empty,
            request.EmergencyContact?.SecondaryPhoneNumber
        ));

        await _driverRepository.SaveAsync(driver);

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

        var saved = await _fileStorage.SaveAsync(
            stream,
            file.FileName,
            file.ContentType,
            cancellationToken);

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