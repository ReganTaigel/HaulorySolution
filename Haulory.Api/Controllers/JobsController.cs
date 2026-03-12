using Haulory.Api.Extensions;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/jobs")]
[Authorize]
public sealed class JobsController : ControllerBase
{
    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly IVehicleAssetRepository _vehicleAssetRepository;

    public JobsController(
        IJobRepository jobRepository,
        IDeliveryReceiptRepository deliveryReceiptRepository,
        IVehicleAssetRepository vehicleAssetRepository)
    {
        _jobRepository = jobRepository;
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _vehicleAssetRepository = vehicleAssetRepository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetAll()
    {
        var ownerUserId = User.GetOwnerUserId();

        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        var result = jobs
            .OrderBy(j => j.SortOrder)
            .Select(j => new
            {
                j.Id,
                j.OwnerUserId,
                j.AssignedToUserId,
                j.SortOrder,

                j.ReferenceNumber,
                j.LoadDescription,

                j.ClientCompanyName,
                j.ClientContactName,
                j.ClientEmail,
                j.ClientAddressLine1,
                j.ClientCity,
                j.ClientCountry,

                j.PickupCompany,
                j.PickupAddress,
                j.DeliveryCompany,
                j.DeliveryAddress,

                j.InvoiceNumber,
                RateType = j.RateType.ToString(),
                j.RateValue,
                j.Quantity,
                Total = j.Total,

                Status = j.Status.ToString(),

                j.DriverId,
                j.VehicleAssetId,

                TrailerAssetIds = j.TrailerAssignments
                    .OrderBy(t => t.Position)
                    .Select(t => t.TrailerAssetId)
                    .ToList(),

                j.ReceiverName,
                j.DeliveredAtUtc,
                j.DeliverySignatureJson,
                j.WaitTimeMinutes,
                j.DamageNotes,
                j.RequiresReview
            });

        return Ok(result);
    }

    [HttpGet("active")]
    [ProducesResponseType(typeof(IEnumerable<object>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<object>>> GetActive()
    {
        var ownerUserId = User.GetOwnerUserId();

        var jobs = await _jobRepository.GetActiveByOwnerAsync(ownerUserId);

        var result = jobs
            .OrderBy(j => j.SortOrder)
            .Select(j => new
            {
                j.Id,
                j.OwnerUserId,
                j.AssignedToUserId,
                j.SortOrder,

                j.ReferenceNumber,
                j.LoadDescription,

                j.ClientCompanyName,
                j.ClientContactName,
                j.ClientEmail,
                j.ClientAddressLine1,
                j.ClientCity,
                j.ClientCountry,

                j.PickupCompany,
                j.PickupAddress,
                j.DeliveryCompany,
                j.DeliveryAddress,

                j.InvoiceNumber,
                RateType = j.RateType.ToString(),
                j.RateValue,
                j.Quantity,
                Total = j.Total,

                Status = j.Status.ToString(),

                j.DriverId,
                j.VehicleAssetId,

                TrailerAssetIds = j.TrailerAssignments
                    .OrderBy(t => t.Position)
                    .Select(t => t.TrailerAssetId)
                    .ToList(),

                j.ReceiverName,
                j.DeliveredAtUtc,
                j.DeliverySignatureJson,
                j.WaitTimeMinutes,
                j.DamageNotes,
                j.RequiresReview
            });

        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<object>> GetById(Guid id)
    {
        var ownerUserId = User.GetOwnerUserId();

        var job = await _jobRepository.GetByIdAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return NotFound();

        return Ok(new
        {
            job.Id,
            job.OwnerUserId,
            job.AssignedToUserId,
            job.SortOrder,

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
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> Create([FromBody] Contracts.Jobs.CreateJobRequest request)
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


        if (request.RateValue < 0)
            ModelState.AddModelError(nameof(request.RateValue), "Rate value cannot be negative.");

        if (request.Quantity < 0)
            ModelState.AddModelError(nameof(request.Quantity), "Quantity cannot be negative.");

        var trailerIds = (request.TrailerAssetIds ?? Array.Empty<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (trailerIds.Count > 2)
            ModelState.AddModelError(nameof(request.TrailerAssetIds), "A maximum of 2 trailers can be assigned to a job.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();


        if (trailerIds.Count > 0)
        {
            var trailerAssets = await _vehicleAssetRepository.GetByIdsAsync(trailerIds);

            if (trailerAssets.Count != trailerIds.Count)
                return BadRequest("One or more selected trailers were not found.");

            if (trailerAssets.Any(x => x.OwnerUserId != ownerUserId))
                return BadRequest("One or more selected trailers do not belong to this owner.");

            if (trailerAssets.Any(x => x.Kind != AssetKind.Trailer))
                return BadRequest("Only trailer assets can be assigned to a job.");
        }

        var sortOrder = await _jobRepository.GetNextSortOrderAsync(ownerUserId);

        string invoiceNumber;
        do
        {
            invoiceNumber = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        }
        while (await _jobRepository.InvoiceNumberExistsAsync(ownerUserId, invoiceNumber));

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
            invoiceNumber: invoiceNumber,
            rateType: request.RateType,
            rateValue: request.RateValue,
            quantity: request.Quantity,

            sortOrder: sortOrder,
            driverId: request.DriverId,
            vehicleAssetId: request.VehicleAssetId
        );

        job.AssignToSubUser(request.AssignedToUserId);
        job.SetTrailers(trailerIds);

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
    public async Task<ActionResult> Complete(Guid id, [FromBody] Contracts.Jobs.CompleteJobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.ReceiverName))
            ModelState.AddModelError(nameof(request.ReceiverName), "Receiver name is required.");

        if (string.IsNullOrWhiteSpace(request.SignatureJson))
            ModelState.AddModelError(nameof(request.SignatureJson), "Signature is required.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();
        var deliveredByUserId = User.GetAccountUserId();

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
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Update(Guid id, [FromBody] Contracts.Jobs.UpdateJobRequest request)
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


        if (request.RateValue < 0)
            ModelState.AddModelError(nameof(request.RateValue), "Rate value cannot be negative.");

        if (request.Quantity < 0)
            ModelState.AddModelError(nameof(request.Quantity), "Quantity cannot be negative.");

        var trailerIds = request.TrailerAssetIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (trailerIds.Count > 2)
            ModelState.AddModelError(nameof(request.TrailerAssetIds), "A maximum of 2 trailers can be assigned to a job.");

        if (!ModelState.IsValid)
            return ValidationProblem(ModelState);

        var ownerUserId = User.GetOwnerUserId();

        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return NotFound();

        if (trailerIds.Count > 0)
        {
            var trailerAssets = await _vehicleAssetRepository.GetByIdsAsync(trailerIds);

            if (trailerAssets.Count != trailerIds.Count)
                return BadRequest("One or more selected trailers were not found.");

            if (trailerAssets.Any(x => x.OwnerUserId != ownerUserId))
                return BadRequest("One or more selected trailers do not belong to this owner.");

            if (trailerAssets.Any(x => x.Kind != AssetKind.Trailer))
                return BadRequest("Only trailer assets can be assigned to a job.");
        }

        // Update core details
        job.UpdateDetails(
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
            invoiceNumber: job.InvoiceNumber,

            rateType: request.RateType,
            rateValue: request.RateValue,
            quantity: request.Quantity,

            driverId: request.DriverId,
            vehicleAssetId: request.VehicleAssetId
        );

        job.AssignToSubUser(request.AssignedToUserId);
        job.SetTrailers(trailerIds);

        await _jobRepository.UpdateAsync(job);

        return Ok(new
        {
            message = "Job updated.",
            jobId = job.Id,
            referenceNumber = job.ReferenceNumber,
            invoiceNumber = job.InvoiceNumber,
            status = job.Status.ToString()
        });
    }


}