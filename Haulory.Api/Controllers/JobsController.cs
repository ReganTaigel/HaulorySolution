using Haulory.Api.Extensions;
using Haulory.Api.Jobs;
using Haulory.Application.Features.Jobs;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Jobs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

// Marks this as an API controller with automatic binding and validation.
[ApiController]

// Base route: api/jobs
[Route("api/jobs")]

// Requires authentication for all job-related endpoints.
[Authorize]
public sealed class JobsController : ControllerBase
{
    // Repository for accessing job data.
    private readonly IJobRepository _jobRepository;

    // Repository for accessing vehicle/trailer assets.
    private readonly IVehicleAssetRepository _vehicleAssetRepository;

    // Validator for incoming job requests.
    private readonly JobRequestValidator _validator;

    // Factory for converting domain entities into API response DTOs.
    private readonly JobResponseFactory _responseFactory;

    // Service responsible for orchestrating job workflows and business logic.
    private readonly JobWorkflowService _workflowService;

    // Constructor injection of dependencies.
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

        // Workflow service is composed here to centralise job business logic.
        _workflowService = new JobWorkflowService(
            jobRepository,
            deliveryReceiptRepository,
            vehicleAssetRepository,
            createJobHandler,
            documentSettingsRepository,
            invoiceCalculationService,
            customerRepository);
    }

    // Returns all active jobs for the current owner.
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<JobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<JobDto>>> GetAll()
    {
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve active jobs scoped to owner.
        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        // Order by defined sort order and map to DTOs.
        return Ok(jobs
            .OrderBy(j => j.SortOrder)
            .Select(_responseFactory.ToDto)
            .ToList());
    }

    // Returns active jobs (same as GetAll - explicit endpoint for clarity).
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

    // Retrieves a specific job by ID if it belongs to the current owner.
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(JobDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<JobDto>> GetById(Guid id)
    {
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve job without filtering, then enforce ownership check.
        var job = await _jobRepository.GetByIdAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return NotFound();

        return Ok(_responseFactory.ToDto(job));
    }

    // Returns available trailer assets for job assignment.
    [HttpGet("trailers")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAvailableTrailers()
    {
        var ownerUserId = User.GetOwnerUserId();

        // Retrieve trailer assets for the current owner.
        var trailers = await _vehicleAssetRepository.GetTrailerAssetsByOwnerAsync(ownerUserId);

        // Format trailers into a UI-friendly display structure.
        var result = trailers.Select(t => new
        {
            t.Id,
            t.Rego,
            t.OdometerKm,

            // Construct readable display name.
            DisplayName = $"{t.TitlePrefix} - {t.Rego} ({t.OdometerKm?.ToString("N0") ?? "—"} km)"
        });

        return Ok(result);
    }

    // Creates a new job.
    [HttpPost]
    [ProducesResponseType(typeof(CreateJobResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create([FromBody] CreateJobRequest request)
    {
        // Validate request and extract trailer IDs.
        var trailerIds = _validator.ValidateCreate(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        try
        {
            // Ensure selected trailers are valid for this owner.
            await _workflowService.ValidateTrailersAsync(ownerUserId, trailerIds);

            // Create job via workflow service.
            var job = await _workflowService.CreateAsync(ownerUserId, request, trailerIds);

            return CreatedAtAction(nameof(GetById), new { id = job.Id }, _responseFactory.ToCreated(job));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    // Completes a job (delivery stage).
    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Complete(Guid id, [FromBody] CompleteJobRequest request)
    {
        // Validate completion request.
        _validator.ValidateComplete(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        // Account user represents the driver or operator performing the action.
        var deliveredByUserId = User.GetAccountUserId();

        try
        {
            // Complete job and generate delivery receipt.
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

    // Updates general job details.
    [HttpPut("{id:guid}")]
    public async Task<ActionResult> Update(Guid id, [FromBody] UpdateJobRequest request)
    {
        // Validate request and extract trailer IDs.
        var trailerIds = _validator.ValidateUpdate(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        try
        {
            // Validate trailer selection.
            await _workflowService.ValidateTrailersAsync(ownerUserId, trailerIds);

            // Update job.
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

    // Updates pickup-related details for a job.
    [HttpPut("{id:guid}/pickup-details")]
    public async Task<ActionResult> UpdatePickupDetails(Guid id, [FromBody] UpdatePickupDetailsRequest request)
    {
        // Validate pickup-specific fields.
        _validator.ValidatePickup(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();
        var accountUserId = User.GetAccountUserId();

        try
        {
            // Update pickup stage via workflow.
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

    // Returns jobs that require review (e.g., post-delivery validation).
    [HttpGet("needs-review")]
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

    // Reviews and finalises a job (e.g., approval, adjustments, invoicing readiness).
    [HttpPut("{id:guid}/review")]
    public async Task<ActionResult> Review(Guid id, [FromBody] ReviewJobRequest request)
    {
        // Validate review request.
        _validator.ValidateReview(request, ModelState);
        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();
        var accountUserId = User.GetAccountUserId();

        try
        {
            // Perform review workflow.
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