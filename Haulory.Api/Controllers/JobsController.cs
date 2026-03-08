using Haulory.Api.Contracts.Jobs;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public sealed class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;

    public JobsController(
        IJobRepository jobRepository,
        IDeliveryReceiptRepository deliveryReceiptRepository)
    {
        _jobRepository = jobRepository;
        _deliveryReceiptRepository = deliveryReceiptRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var ownerUserId = GetOwnerUserId();

        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        var result = jobs.Select(j => new
        {
            j.Id,
            j.ReferenceNumber,
            j.ClientCompanyName,
            j.PickupCompany,
            j.PickupAddress,
            j.DeliveryCompany,
            j.DeliveryAddress,
            Status = j.Status.ToString(),
            j.InvoiceNumber,
            j.ReceiverName,
            j.DeliveredAtUtc,
            j.DriverId,
            j.VehicleAssetId
        });

        return Ok(result);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetActive()
    {
        var ownerUserId = GetOwnerUserId();

        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        var result = jobs.Select(j => new
        {
            j.Id,
            j.ReferenceNumber,
            j.PickupCompany,
            j.PickupAddress,
            j.DeliveryCompany,
            j.DeliveryAddress,
            Status = j.Status.ToString(),
            j.DriverId,
            j.VehicleAssetId
        });

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetById(Guid id)
    {
        var ownerUserId = GetOwnerUserId();

        var job = await _jobRepository.GetByIdAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return NotFound();

        return Ok(new
        {
            job.Id,
            job.OwnerUserId,
            job.ReferenceNumber,
            job.LoadDescription,
            job.ClientCompanyName,
            job.ClientContactName,
            job.ClientEmail,
            job.ClientAddressLine1,
            job.ClientCity,
            job.ClientCountry,
            job.PickupCompany,
            job.PickupAddress,
            job.DeliveryCompany,
            job.DeliveryAddress,
            job.InvoiceNumber,
            RateType = job.RateType.ToString(),
            job.RateValue,
            job.Quantity,
            Total = job.Total,
            Status = job.Status.ToString(),
            job.DriverId,
            job.VehicleAssetId,
            TrailerAssetIds = job.TrailerAssignments
                .OrderBy(t => t.Position)
                .Select(t => t.TrailerAssetId)
                .ToList(),
            job.ReceiverName,
            job.DeliveredAtUtc,
            job.DeliverySignatureJson,
            job.WaitTimeMinutes,
            job.DamageNotes,
            job.RequiresReview
        });
    }

    [HttpPost]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create([FromBody] CreateJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ClientCompanyName))
            ModelState.AddModelError(nameof(request.ClientCompanyName), "Client company name is required.");

        if (string.IsNullOrWhiteSpace(request.ClientAddressLine1))
            ModelState.AddModelError(nameof(request.ClientAddressLine1), "Client address is required.");

        if (string.IsNullOrWhiteSpace(request.ClientCity))
            ModelState.AddModelError(nameof(request.ClientCity), "Client city is required.");

        if (string.IsNullOrWhiteSpace(request.ClientCountry))
            ModelState.AddModelError(nameof(request.ClientCountry), "Client country is required.");

        if (string.IsNullOrWhiteSpace(request.PickupCompany))
            ModelState.AddModelError(nameof(request.PickupCompany), "Pickup company is required.");

        if (string.IsNullOrWhiteSpace(request.PickupAddress))
            ModelState.AddModelError(nameof(request.PickupAddress), "Pickup address is required.");

        if (string.IsNullOrWhiteSpace(request.DeliveryCompany))
            ModelState.AddModelError(nameof(request.DeliveryCompany), "Delivery company is required.");

        if (string.IsNullOrWhiteSpace(request.DeliveryAddress))
            ModelState.AddModelError(nameof(request.DeliveryAddress), "Delivery address is required.");

        if (string.IsNullOrWhiteSpace(request.InvoiceNumber))
            ModelState.AddModelError(nameof(request.InvoiceNumber), "Invoice number is required.");

        if (request.RateValue < 0)
            ModelState.AddModelError(nameof(request.RateValue), "Rate value cannot be negative.");

        if (request.Quantity < 0)
            ModelState.AddModelError(nameof(request.Quantity), "Quantity cannot be negative.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = GetOwnerUserId();

        var invoiceExists = await _jobRepository.InvoiceNumberExistsAsync(
            ownerUserId,
            request.InvoiceNumber.Trim());

        if (invoiceExists)
            return BadRequest($"Invoice number already exists: {request.InvoiceNumber}");

        var sortOrder = await _jobRepository.GetNextSortOrderAsync(ownerUserId);

        var job = new Job(
            jobId: Guid.NewGuid(),
            ownerUserId: ownerUserId,

            clientCompanyName: request.ClientCompanyName,
            clientContactName: request.ClientContactName,
            clientEmail: request.ClientEmail,
            clientAddressLine1: request.ClientAddressLine1,
            clientCity: request.ClientCity,
            clientCountry: request.ClientCountry,

            pickupCompany: request.PickupCompany,
            pickupAddress: request.PickupAddress,
            deliveryCompany: request.DeliveryCompany,
            deliveryAddress: request.DeliveryAddress,

            referenceNumber: request.ReferenceNumber,
            loadDescription: request.LoadDescription,
            invoiceNumber: request.InvoiceNumber,

            rateType: request.RateType,
            rateValue: request.RateValue,
            quantity: request.Quantity,

            sortOrder: sortOrder,
            driverId: request.DriverId,
            vehicleAssetId: request.VehicleAssetId
        );

        job.SetTrailers(request.TrailerAssetIds);

        await _jobRepository.AddAsync(job);

        return CreatedAtAction(
            nameof(GetById),
            new { id = job.Id },
            new
            {
                message = "Job created.",
                jobId = job.Id,
                referenceNumber = job.ReferenceNumber,
                invoiceNumber = job.InvoiceNumber,
                status = job.Status.ToString()
            });
    }

    [HttpPost("{id:guid}/complete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Complete(Guid id, [FromBody] CompleteJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReceiverName))
            ModelState.AddModelError(nameof(request.ReceiverName), "Receiver name is required.");

        if (string.IsNullOrWhiteSpace(request.SignatureJson))
            ModelState.AddModelError(nameof(request.SignatureJson), "Signature is required.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = GetOwnerUserId();
        var deliveredByUserId = GetDeliveredByUserId();

        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return NotFound();

        var existingReceipts = await _deliveryReceiptRepository.GetByJobIdAsync(ownerUserId, job.Id);
        if (existingReceipts.Any())
            return BadRequest("A delivery receipt already exists for this job.");

        job.CompleteDelivery(
            deliveredByUserId: deliveredByUserId,
            receiverName: request.ReceiverName,
            signatureJson: request.SignatureJson,
            waitTimeMinutes: request.WaitTimeMinutes,
            damageNotes: request.DamageNotes);

        await _jobRepository.UpdateAsync(job);

        var receipt = new DeliveryReceipt(
            ownerUserId: job.OwnerUserId,
            jobId: job.Id,

            clientCompanyName: job.ClientCompanyName,
            clientContactName: job.ClientContactName,
            clientEmail: job.ClientEmail,
            clientAddressLine1: job.ClientAddressLine1,
            clientCity: job.ClientCity,
            clientCountry: job.ClientCountry,

            referenceNumber: job.ReferenceNumber,
            invoiceNumber: job.InvoiceNumber,
            pickupCompany: job.PickupCompany,
            pickupAddress: job.PickupAddress,
            deliveryCompany: job.DeliveryCompany,
            deliveryAddress: job.DeliveryAddress,
            loadDescription: job.LoadDescription,
            rateType: job.RateType,
            rateValue: job.RateValue,
            quantity: job.Quantity,
            total: job.Total,
            receiverName: job.ReceiverName ?? request.ReceiverName,
            deliveredAtUtc: job.DeliveredAtUtc ?? DateTime.UtcNow,
            signatureJson: job.DeliverySignatureJson ?? request.SignatureJson
        );

        await _deliveryReceiptRepository.AddAsync(receipt);

        return Ok(new
        {
            message = "Job completed.",
            jobId = job.Id,
            status = job.Status.ToString(),
            requiresReview = job.RequiresReview,
            receiptId = receipt.Id
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

    private Guid GetDeliveredByUserId()
    {
        var value =
            User.FindFirstValue(ClaimTypes.NameIdentifier) ??
            User.FindFirstValue("sub") ??
            User.FindFirstValue("userId");

        if (!Guid.TryParse(value, out var deliveredByUserId))
            throw new UnauthorizedAccessException("Authenticated user id is missing or invalid.");

        return deliveredByUserId;
    }
}