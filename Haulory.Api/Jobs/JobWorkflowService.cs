using Haulory.Application.Features.Jobs;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Contracts.Customers;
using Haulory.Contracts.Jobs;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Api.Jobs;

public sealed class JobWorkflowService
{
    private readonly IJobRepository _jobRepository;
    private readonly IDeliveryReceiptRepository _deliveryReceiptRepository;
    private readonly IVehicleAssetRepository _vehicleAssetRepository;
    private readonly CreateJobHandler _createJobHandler;
    private readonly IDocumentSettingsRepository _documentSettingsRepository;
    private readonly IInvoiceCalculationService _invoiceCalculationService;
    private readonly ICustomerRepository _customerRepository;

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

    public async Task<Job> CreateAsync(Guid ownerUserId, CreateJobRequest request, List<Guid> trailerIds)
    {
        var jobId = Guid.NewGuid();

        var latestInvoiceNumber = await _jobRepository.GetLatestInvoiceNumberAsync(ownerUserId);
        var invoiceNumber = InvoiceNumberGenerator.GetNext(latestInvoiceNumber);

        while (await _jobRepository.InvoiceNumberExistsAsync(ownerUserId, invoiceNumber))
            invoiceNumber = InvoiceNumberGenerator.Increment(invoiceNumber);

        if (string.IsNullOrWhiteSpace(invoiceNumber))
            throw new InvalidOperationException("Failed to generate invoice number.");

        var customerId = await EnsureCustomerAsync(
            ownerUserId,
            request.CustomerId,
            request.ClientCompanyName,
            request.ClientContactName,
            request.ClientEmail,
            request.ClientAddressLine1,
            request.ClientCity,
            request.ClientCountry);

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

        var created = await _jobRepository.GetByIdAsync(jobId)
            ?? throw new InvalidOperationException("Job was created but could not be reloaded.");

        return created;
    }

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
        job.SetTrailers(trailerIds);
        job.UpdatePickupDetails(request.WaitTimeMinutes, request.DamageNotes);

        await _jobRepository.UpdateAsync(job);
        return job;
    }

    public async Task<Job?> UpdatePickupDetailsAsync(Guid ownerUserId, Guid accountUserId, Guid id, UpdatePickupDetailsRequest request)
    {
        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return null;

        var isMainUser = ownerUserId == accountUserId;
        if (!isMainUser && job.AssignedToUserId != accountUserId)
            throw new UnauthorizedAccessException();

        job.UpdatePickupDetails(request.WaitTimeMinutes, request.DamageNotes);
        await _jobRepository.UpdateAsync(job);
        return job;
    }

    public async Task<(Job? job, Guid? receiptId)> CompleteAsync(Guid ownerUserId, Guid deliveredByUserId, Guid id, CompleteJobRequest request)
    {
        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return (null, null);

        var existingReceipts = await _deliveryReceiptRepository.GetByJobIdAsync(ownerUserId, job.Id);
        if (existingReceipts.Any())
            throw new InvalidOperationException("A delivery receipt already exists for this job.");

        job.CompleteDelivery(
            deliveredByUserId,
            request.ReceiverName,
            request.SignatureJson,
            request.WaitTimeMinutes,
            request.DamageNotes);

        await _jobRepository.UpdateAsync(job);

        Guid? receiptId = null;
        if (job.Status == JobStatus.Completed)
            receiptId = await EnsureDeliveryReceiptAsync(job, request.ReceiverName, request.SignatureJson);

        return (job, receiptId);
    }

    public async Task<IReadOnlyList<Job>> GetNeedsReviewAsync(Guid ownerUserId, Guid accountUserId)
    {
        if (ownerUserId != accountUserId)
            throw new UnauthorizedAccessException();

        return await _jobRepository.GetNeedsReviewAsync(ownerUserId);
    }

    public async Task<Job?> ReviewAsync(Guid ownerUserId, Guid accountUserId, Guid id, ReviewJobRequest request)
    {
        if (ownerUserId != accountUserId)
            throw new UnauthorizedAccessException();

        var job = await _jobRepository.GetByIdForUpdateAsync(id);

        if (job is null || job.OwnerUserId != ownerUserId)
            return null;

        if (job.Status != JobStatus.DeliveredPendingReview)
            throw new InvalidOperationException("Job is not awaiting review.");

        job.ReviewDeliveryExceptions(request.WaitTimeMinutes, request.DamageNotes);
        await _jobRepository.UpdateAsync(job);
        await EnsureDeliveryReceiptAsync(job, job.ReceiverName ?? string.Empty, job.DeliverySignatureJson ?? string.Empty);

        return job;
    }

    private async Task<Guid?> EnsureDeliveryReceiptAsync(Job job, string receiverName, string signatureJson)
    {
        var existingReceipts = await _deliveryReceiptRepository.GetByJobIdAsync(job.OwnerUserId, job.Id);

        var settings = await _documentSettingsRepository.GetOrCreateAsync(job.OwnerUserId);

        var calc = _invoiceCalculationService.Calculate(
            rateValue: job.RateValue,
            quantity: job.Quantity,
            gstEnabled: settings.GstEnabled,
            gstRatePercent: settings.GstRatePercent,
            fuelSurchargeEnabled: settings.FuelSurchargeEnabled,
            fuelSurchargePercent: settings.FuelSurchargePercent);

        var existing = existingReceipts.FirstOrDefault();
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
                damageNotes: job.DamageNotes);

            await _deliveryReceiptRepository.UpdateAsync(existing);
            return existing.Id;
        }

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
            damageNotes: job.DamageNotes);

        await _deliveryReceiptRepository.AddAsync(receipt);
        return receipt.Id;
    }

    private async Task<Guid?> EnsureCustomerAsync(Guid ownerUserId, Guid? customerId, string companyName, string? contactName, string? email, string addressLine1, string city,  string country)
        {
            if (string.IsNullOrWhiteSpace(companyName) ||
                string.IsNullOrWhiteSpace(addressLine1) ||
                string.IsNullOrWhiteSpace(city) ||
                string.IsNullOrWhiteSpace(country))
            {
                return customerId;
            }

            if (customerId.HasValue && customerId.Value != Guid.Empty)
            {
                var existing = await _customerRepository.GetByIdForUpdateAsync(customerId.Value);
                if (existing == null || existing.OwnerUserId != ownerUserId)
                    throw new InvalidOperationException("Selected customer was not found.");

                existing.UpdateDetails(companyName, contactName, email, addressLine1, city, country);
                await _customerRepository.UpdateAsync(existing);
                return existing.Id;
            }

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
