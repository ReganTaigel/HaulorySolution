using Haulory.Api.Contracts.Drivers;
using Haulory.Api.Extensions;
using Haulory.Application.Features.Drivers;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/drivers")]
[Authorize]
public sealed class DriversController : ControllerBase
{
    private readonly IDriverRepository _driverRepository;
    private readonly CreateDriverHandler _createDriverHandler;

    public DriversController(
        IDriverRepository driverRepository,
        CreateDriverHandler createDriverHandler)
    {
        _driverRepository = driverRepository;
        _createDriverHandler = createDriverHandler;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<DriverDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<DriverDto>>> GetAll()
    {
        var ownerUserId = GetOwnerUserId();

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
        var ownerUserId = GetOwnerUserId();

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
            ModelState.AddModelError(nameof(request.EmergencyContact), "Emergency contact is required.");

        if (request.CreateLoginAccount && string.IsNullOrWhiteSpace(request.Password))
            ModelState.AddModelError(nameof(request.Password), "Password is required when creating a login account.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = GetOwnerUserId();

        var created = await _createDriverHandler.HandleAsync(
            new CreateDriverCommand(
                OwnerUserId: ownerUserId,
                FirstName: request.FirstName,
                LastName: request.LastName,
                Email: request.Email,
                LicenceNumber: request.LicenceNumber,

                // Contact + profile
                PhoneNumber: request.PhoneNumber,
                DateOfBirthUtc: request.DateOfBirthUtc,

                // Licence
                LicenceExpiresOnUtc: request.LicenceExpiresOnUtc,
                LicenceVersion: request.LicenceVersion,
                LicenceClassOrEndorsements: request.LicenceClassOrEndorsements,
                LicenceIssuedOnUtc: request.LicenceIssuedOnUtc,
                LicenceConditionsNotes: request.LicenceConditionsNotes,

                // Address
                Line1: request.Line1,
                Line2: request.Line2,
                Suburb: request.Suburb,
                City: request.City,
                Region: request.Region,
                Postcode: request.Postcode,
                Country: request.Country,

                // Emergency Contact
                EmergencyFirstName: request.EmergencyContact?.FirstName ?? string.Empty,
                EmergencyLastName: request.EmergencyContact?.LastName ?? string.Empty,
                EmergencyRelationship: request.EmergencyContact?.Relationship ?? string.Empty,
                EmergencyEmail: request.EmergencyContact?.Email ?? string.Empty,
                EmergencyPhoneNumber: request.EmergencyContact?.PhoneNumber ?? string.Empty,
                EmergencySecondaryPhoneNumber: request.EmergencyContact?.SecondaryPhoneNumber,

                // Optional login account
                CreateLoginAccount: request.CreateLoginAccount,
                Password: request.Password
            ));

        if (created is null)
            return BadRequest("Unable to create driver.");

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created.ToDto());
    }

    private Guid GetOwnerUserId()
    {
        var value =
            User.FindFirstValue("owner_id") ??
            User.FindFirstValue("ownerId") ??
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId");

        if (!Guid.TryParse(value, out var ownerUserId))
            throw new UnauthorizedAccessException("Authenticated owner id is missing or invalid.");

        return ownerUserId;
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

        var ownerUserId = GetOwnerUserId();

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
}