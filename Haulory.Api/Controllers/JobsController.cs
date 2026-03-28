using Haulory.Api.Extensions;
using Haulory.Api.Jobs;
using Haulory.Application.Features.Jobs;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public sealed class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly IVehicleAssetRepository _vehicleAssetRepository;
    private readonly JobRequestValidator _validator;
    private readonly JobResponseFactory _responseFactory;
    private readonly JobWorkflowService _workflowService;

    public JobsController(
        IJobRepository jobRepository,
        IDeliveryReceiptRepository deliveryReceiptRepository,
        IVehicleAssetRepository vehicleAssetRepository,
        CreateJobHandler createJobHandler,
        IDocumentSettingsRepository documentSettingsRepository,
        IInvoiceCalculationService invoiceCalculationService,
        JobRequestValidator validator,
        JobResponseFactory responseFactory,
        ICustomerRepository customerRepository)
    {
        _jobRepository = jobRepository;
        _vehicleAssetRepository = vehicleAssetRepository;
        _validator = validator;
        _responseFactory = responseFactory;

        _workflowService = new JobWorkflowService(
            jobRepository,
            deliveryReceiptRepository,
            vehicleAssetRepository,
            createJobHandler,
            documentSettingsRepository,
            invoiceCalculationService,
            customerRepository);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetAll()
    {
        var ownerUserId = User.GetOwnerUserId();
        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        return Ok(jobs
            .OrderBy(j => j.SortOrder)
            .Select(_responseFactory.ToDto)
            .ToList());
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<JobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetActive()
    {
        var ownerUserId = User.GetOwnerUserId();
        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        return Ok(jobs
            .OrderBy(j => j.SortOrder)
            .Select(_responseFactory.ToDto)
            .ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobDto>> GetById(Guid id)
    {
        var ownerUserId = User.GetOwnerUserId();
        var job = await _jobRepository.GetByIdAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return NotFound();

        return Ok(_responseFactory.ToDto(job));
    }

    [HttpGet("trailers")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAvailableTrailers()
    {
        var ownerUserId = User.GetOwnerUserId();
        var trailers = await _vehicleAssetRepository.GetTrailerAssetsByOwnerAsync(ownerUserId);

        var result = trailers.Select(t => new
        {
            t.Id,
            t.Rego,
            t.OdometerKm,
            DisplayName = $"{t.TitlePrefix} - {t.Rego} ({t.OdometerKm?.ToString("N0") ?? "—"} km)"
        });

        return Ok(result);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateJobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create([FromBody] CreateJobRequest request)
    {
        var trailerIds = _validator.ValidateCreate(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        try
        {
            await _workflowService.ValidateTrailersAsync(ownerUserId, trailerIds);
            var job = await _workflowService.CreateAsync(ownerUserId, request, trailerIds);
            return CreatedAtAction(nameof(GetById), new { id = job.Id }, _responseFactory.ToCreated(job));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Complete(Guid id, [FromBody] CompleteJobRequest request)
    {
        _validator.ValidateComplete(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();
        var deliveredByUserId = User.GetAccountUserId();

        try
        {
            var (job, receiptId) = await _workflowService.CompleteAsync(ownerUserId, deliveredByUserId, id, request);
            if (job is null)
                return NotFound();

            return Ok(_responseFactory.ToCompleted(job, receiptId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateJobRequest request)
    {
        var trailerIds = _validator.ValidateUpdate(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        try
        {
            await _workflowService.ValidateTrailersAsync(ownerUserId, trailerIds);
            var job = await _workflowService.UpdateAsync(ownerUserId, id, request, trailerIds);
            if (job is null)
                return NotFound();

            return Ok(_responseFactory.ToUpdated(job));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id:guid}/pickup-details")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> UpdatePickupDetails(Guid id, [FromBody] UpdatePickupDetailsRequest request)
    {
        _validator.ValidatePickup(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();
        var accountUserId = User.GetAccountUserId();

        try
        {
            var job = await _workflowService.UpdatePickupDetailsAsync(ownerUserId, accountUserId, id, request);
            if (job is null)
                return NotFound();

            return Ok(_responseFactory.ToPickupUpdated(job));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpGet("needs-review")]
    [ProducesResponseType(typeof(IEnumerable<JobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetNeedsReview()
    {
        var ownerUserId = User.GetOwnerUserId();
        var accountUserId = User.GetAccountUserId();

        try
        {
            var jobs = await _workflowService.GetNeedsReviewAsync(ownerUserId, accountUserId);
            return Ok(jobs
                .OrderByDescending(j => j.DeliveredAtUtc)
                .Select(_responseFactory.ToDto)
                .ToList());
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
    }

    [HttpPut("{id:guid}/review")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Review(Guid id, [FromBody] ReviewJobRequest request)
    {
        _validator.ValidateReview(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();
        var accountUserId = User.GetAccountUserId();

        try
        {
            var job = await _workflowService.ReviewAsync(ownerUserId, accountUserId, id, request);
            if (job is null)
                return NotFound();

            return Ok(_responseFactory.ToReviewed(job));
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}