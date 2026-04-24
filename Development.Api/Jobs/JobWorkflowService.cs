using HaulitCore.Application.Features.Jobs;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Contracts.Customers;
using HaulitCore.Contracts.Jobs;
using HaulitCore.Domain.Entities;
using HaulitCore.Domain.Enums;

namespace HaulitCore.Api.Jobs;

// Central orchestration service for job workflows.
// Coordinates multiple repositories and services to execute business logic.
public sealed class JobWorkflowService
{
    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly IVehicleAssetRepository _vehicleAssetRepository;
    private readonly CreateJobHandler _createJobHandler;
    private readonly IDocumentSettingsRepository _documentSettingsRepository;
    private readonly IInvoiceCalculationService _invoiceCalculationService;
    private readonly ICustomerRepository _customerRepository;

    // Constructor injection of all required dependencies.
    public JobWorkflowService(
        IJobRepository jobRepository,
        IDeliveryReceiptRepository deliveryReceiptRepository,
        IVehicleAssetRepository vehicleAssetRepository,
        CreateJobHandler createJobHandler,
        IDocumentSettingsRepository documentSettingsRepository,
        IInvoiceCalculationService invoiceCalculationService,
        ICustomerRepository customerRepository)
    {
        _jobRepository = jobRepository;
        _deliveryReceiptRepository = deliveryReceiptRepository;
        _vehicleAssetRepository = vehicleAssetRepository;
        _createJobHandler = createJobHandler;
        _documentSettingsRepository = documentSettingsRepository;
        _invoiceCalculationService = invoiceCalculationService;
        _customerRepository = customerRepository;
    }

    // Validates that selected trailer assets exist, belong to the owner, and are valid trailer types.
    public async Task<List<Guid>?> ValidateTrailersAsync(Guid ownerUserId, List<Guid> trailerIds)
    {
        if (trailerIds.Count == 0)
            return trailerIds;

        var trailerAssets = await _vehicleAssetRepository.GetByIdsAsync(trailerIds);

        if (trailerAssets.Count != trailerIds.Count)
            throw new InvalidOperationException("One or more selected trailers were not found.");

        if (trailerAssets.Any(x => x.OwnerUserId != ownerUserId))
            throw new InvalidOperationException("One or more selected trailers do not belong to this owner.");

        if (trailerAssets.Any(x => x.Kind != AssetKind.Trailer))
            throw new InvalidOperationException("Only trailer assets can be assigned to a job.");

        return trailerIds;
    }

    // Creates a new job and assigns a unique invoice number.
    public async Task<Job> CreateAsync(Guid ownerUserId, CreateJobRequest request, List<Guid> trailerIds)
    {
        var jobId = Guid.NewGuid();

        // Generate next invoice number based on existing records.
        var latestInvoiceNumber = await _jobRepository.GetLatestInvoiceNumberAsync(ownerUserId);
        var invoiceNumber = InvoiceNumberGenerator.GetNext(latestInvoiceNumber);

        // Ensure invoice number is unique.
        while (await _jobRepository.InvoiceNumberExistsAsync(ownerUserId, invoiceNumber))
            invoiceNumber = InvoiceNumberGenerator.Increment(invoiceNumber);

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new InvalidOperationException("Failed to generate invoice number.");

        // Ensure a valid customer exists (create/update if necessary).
        var customerId = await EnsureCustomerAsync(
            ownerUserId,
            request.CustomerId,
            request.ClientCompanyName,
            request.ClientContactName,
            request.ClientEmail,
            request.ClientAddressLine1,
            request.ClientCity,
            request.ClientCountry);

        // Delegate creation to handler.
        await _createJobHandler.HandleAsync(
            new CreateJobCommand(
                ownerUserId,
                customerId,
                jobId,
                request.ClientCompanyName,
                request.ClientContactName,
                request.ClientEmail,
                request.ClientAddressLine1,
                request.ClientCity,
                request.ClientCountry,
                request.PickupCompany,
                request.PickupAddress,
                request.DeliveryCompany,
                request.DeliveryAddress,
                request.ReferenceNumber,
                request.LoadDescription,
                invoiceNumber,
                request.RateType,
                request.Quantity,
                request.RateValue,
                request.DriverId,
                request.VehicleAssetId,
                request.AssignedToUserId,
                trailerIds
            )
        );

        // Reload the created job.
        var created = await _jobRepository.GetByIdAsync(jobId)
            ?? throw new InvalidOperationException("Job was created but could not be reloaded.");

        return created;
    }

    // Updates an existing job.
    public async Task<Job?> UpdateAsync(Guid ownerUserId, Guid id, UpdateJobRequest request, List<Guid> trailerIds)
    {
        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return null;

        var customerId = await EnsureCustomerAsync(
            ownerUserId,
            request.CustomerId,
            request.ClientCompanyName,
            request.ClientContactName,
            request.ClientEmail,
            request.ClientAddressLine1,
            request.ClientCity,
            request.ClientCountry);

        job.UpdateDetails(
            customerId,
            request.ClientCompanyName,
            request.ClientContactName,
            request.ClientEmail,
            request.ClientAddressLine1,
            request.ClientCity,
            request.ClientCountry,
            request.PickupCompany,
            request.PickupAddress,
            request.DeliveryCompany,
            request.DeliveryAddress,
            request.ReferenceNumber,
            request.LoadDescription,
            job.InvoiceNumber,
            request.RateType,
            request.RateValue,
            request.Quantity,
            request.DriverId,
            request.VehicleAssetId);

        job.AssignToSubUser(request.AssignedToUserId);
        job.UpdatePickupDetails(request.WaitTimeMinutes, request.DamageNotes);

        await _jobRepository.UpdateAsync(job);
        await _jobRepository.SyncTrailerAssignmentsAsync(job.Id, trailerIds);

        return await _jobRepository.GetByIdAsync(job.Id);
    }

    // Updates pickup details with access control (driver or owner).
    public async Task<Job?> UpdatePickupDetailsAsync(Guid ownerUserId, Guid accountUserId, Guid id, UpdatePickupDetailsRequest request)
    {
        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return null;

        // Only owner or assigned user can update.
        var isMainUser = ownerUserId == accountUserId;
        if (!isMainUser && job.AssignedToUserId != accountUserId)
            throw new UnauthorizedAccessException();

        job.UpdatePickupDetails(request.WaitTimeMinutes, request.DamageNotes);
        await _jobRepository.UpdateAsync(job);

        return job;
    }

    // Completes a job and optionally generates a delivery receipt.
    public async Task<(Job? job, Guid? receiptId)> CompleteAsync(Guid ownerUserId, Guid deliveredByUserId, Guid id, CompleteJobRequest request)
    {
        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return (null, null);

        // Prevent duplicate receipts.
        var existingReceipts = await _deliveryReceiptRepository.GetByJobIdAsync(ownerUserId, job.Id);
        if (existingReceipts.Any())
            throw new InvalidOperationException("A delivery receipt already exists for this job.");

        // Mark job as completed.
        job.CompleteDelivery(
            deliveredByUserId,
            request.ReceiverName,
            request.SignatureJson,
            request.WaitTimeMinutes,
            request.DamageNotes);

        await _jobRepository.UpdateAsync(job);

        Guid? receiptId = null;

        // Only generate receipt if job is fully completed (not pending review).
        if (job.Status == JobStatus.Completed)
            receiptId = await EnsureDeliveryReceiptAsync(job, request.ReceiverName, request.SignatureJson);

        return (job, receiptId);
    }

    // Returns jobs requiring review (owner only).
    public async Task<IReadOnlyList<Job>> GetNeedsReviewAsync(Guid ownerUserId, Guid accountUserId)
    {
        if (ownerUserId != accountUserId)
            throw new UnauthorizedAccessException();

        return await _jobRepository.GetNeedsReviewAsync(ownerUserId);
    }

    // Reviews and finalises a job.
    public async Task<Job?> ReviewAsync(Guid ownerUserId, Guid accountUserId, Guid id, ReviewJobRequest request)
    {
        if (ownerUserId != accountUserId)
            throw new UnauthorizedAccessException();

        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return null;

        if (job.Status != JobStatus.DeliveredPendingReview)
            throw new InvalidOperationException("Job is not awaiting review.");

        // Apply review adjustments.
        job.ReviewDeliveryExceptions(request.WaitTimeMinutes, request.DamageNotes);
        await _jobRepository.UpdateAsync(job);

        // Ensure receipt is created after review.
        await EnsureDeliveryReceiptAsync(job, job.ReceiverName ?? string.Empty, job.DeliverySignatureJson ?? string.Empty);

        return job;
    }

    // Ensures a delivery receipt exists and is up-to-date.
    private async Task<Guid?> EnsureDeliveryReceiptAsync(Job job, string receiverName, string signatureJson)
    {
        var existingReceipts = await _deliveryReceiptRepository.GetByJobIdAsync(job.OwnerUserId, job.Id);

        // Load document settings for calculations.
        var settings = await _documentSettingsRepository.GetOrCreateAsync(job.OwnerUserId);

        // Calculate invoice values.
        var calc = _invoiceCalculationService.Calculate(
            rateValue: job.RateValue,
            quantity: job.Quantity,
            gstEnabled: settings.GstEnabled,
            gstRatePercent: settings.GstRatePercent,
            fuelSurchargeEnabled: settings.FuelSurchargeEnabled,
            fuelSurchargePercent: settings.FuelSurchargePercent, 
            waitTimeChargeEnabled: settings.WaitTimeChargeEnabled,
            waitTimeChargeAmount: settings.WaitTimeCharge,
            handUnloadChargeEnabled: settings.HandUnloadChargeEnabled,
            handUnloadChargeAmount: settings.HandUnloadCharge);

        var existing = existingReceipts.FirstOrDefault();

        // Update existing receipt if present.
        if (existing != null)
        {
            existing.RefreshSnapshot(
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
                subtotal: calc.Subtotal,
                gstEnabled: settings.GstEnabled,
                gstRatePercent: settings.GstRatePercent,
                gstAmount: calc.GstAmount,
                fuelSurchargeEnabled: settings.FuelSurchargeEnabled,
                fuelSurchargePercent: settings.FuelSurchargePercent,
                fuelSurchargeAmount: calc.FuelSurchargeAmount,
                total: calc.Total,
                receiverName: job.ReceiverName ?? receiverName,
                deliveredAtUtc: job.DeliveredAtUtc ?? DateTime.UtcNow,
                signatureJson: job.DeliverySignatureJson ?? signatureJson,
                waitTimeMinutes: job.WaitTimeMinutes,
                waitTimeChargeEnabled: settings.WaitTimeChargeEnabled,
                waitTimeChargeAmount: calc.WaitTimeChargeAmount,
                handUnloadChargeEnabled: settings.HandUnloadChargeEnabled,
                handUnloadChargeAmount: calc.HandUnloadChargeAmount,
                damageNotes: job.DamageNotes);

            await _deliveryReceiptRepository.UpdateAsync(existing);
            return existing.Id;
        }

        // Create new receipt if none exists.
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
            subtotal: calc.Subtotal,
            gstEnabled: settings.GstEnabled,
            gstRatePercent: settings.GstRatePercent,
            gstAmount: calc.GstAmount,
            fuelSurchargeEnabled: settings.FuelSurchargeEnabled,
            fuelSurchargePercent: settings.FuelSurchargePercent,
            fuelSurchargeAmount: calc.FuelSurchargeAmount,
            total: calc.Total,
            receiverName: job.ReceiverName ?? receiverName,
            deliveredAtUtc: job.DeliveredAtUtc ?? DateTime.UtcNow,
            signatureJson: job.DeliverySignatureJson ?? signatureJson,
            waitTimeMinutes: job.WaitTimeMinutes,
            waitTimeChargeEnabled: settings.WaitTimeChargeEnabled,
            waitTimeChargeAmount: calc.WaitTimeChargeAmount,
            handUnloadChargeEnabled: settings.HandUnloadChargeEnabled,
            handUnloadChargeAmount: calc.HandUnloadChargeAmount,
            damageNotes: job.DamageNotes);

        await _deliveryReceiptRepository.AddAsync(receipt);
        return receipt.Id;
    }

    // Ensures a valid customer exists or creates/updates one.
    private async Task<Guid?> EnsureCustomerAsync(Guid ownerUserId, Guid? customerId, string companyName, string? contactName, string? email, string addressLine1, string city, string country)
    {
        // If insufficient data, return existing ID.
        if (string.IsNullOrWhiteSpace(companyName) ||
            string.IsNullOrWhiteSpace(addressLine1) ||
            string.IsNullOrWhiteSpace(city) ||
            string.IsNullOrWhiteSpace(country))
        {
            return customerId;
        }

        // Update existing customer if provided.
        if (customerId.HasValue && customerId.Value != Guid.Empty)
        {
            var existing = await _customerRepository.GetByIdForUpdateAsync(customerId.Value);
            if (existing == null || existing.OwnerUserId != ownerUserId)
                throw new InvalidOperationException("Selected customer was not found.");

            existing.UpdateDetails(companyName, contactName, email, addressLine1, city, country);
            await _customerRepository.UpdateAsync(existing);
            return existing.Id;
        }

        // Try to match existing customer.
        var matched = await _customerRepository.FindMatchAsync(
            ownerUserId,
            companyName,
            email,
            addressLine1,
            city,
            country);

        if (matched != null)
        {
            matched.UpdateDetails(companyName, contactName, email, addressLine1, city, country);
            await _customerRepository.UpdateAsync(matched);
            return matched.Id;
        }

        // Create new customer.
        var customer = new Customer(
            Guid.NewGuid(),
            ownerUserId,
            companyName,
            contactName,
            email,
            addressLine1,
            city,
            country);

        await _customerRepository.AddAsync(customer);
        return customer.Id;
    }
}