using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Jobs;

// Handles the application use case for creating a new job.
// Responsible for validating trailer assignments, constructing the Job entity,
// and saving it through the repository.
public class CreateJobHandler
{
    // Repository used to persist jobs and retrieve sort-order information.
    private readonly IJobRepository _jobRepository;

    // Repository used to validate selected trailer assets.
    private readonly IVehicleAssetRepository _vehicleAssetRepository;

    // Constructor injection of dependencies.
    public CreateJobHandler(
        IJobRepository jobRepository,
        IVehicleAssetRepository vehicleAssetRepository)
    {
        _jobRepository = jobRepository;
        _vehicleAssetRepository = vehicleAssetRepository;
    }

    // Creates a new job from the supplied command.
    public async Task HandleAsync(CreateJobCommand command)
    {
        // Owner context is required for multi-tenant safety.
        if (command.OwnerUserId == Guid.Empty)
            throw new InvalidOperationException("Owner user is required.");

        // Normalise trailer IDs by removing empty values and duplicates.
        var trailerIds = command.TrailerAssetIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? new List<Guid>();

        // Enforce business rule: a job may have at most 2 trailers.
        if (trailerIds.Count > 2)
            throw new InvalidOperationException("A maximum of 2 trailers can be assigned to a job.");

        // Validate trailer assets if any were supplied.
        if (trailerIds.Count > 0)
        {
            var trailerAssets = await _vehicleAssetRepository.GetByIdsAsync(trailerIds);

            // Ensure all requested trailer assets exist.
            if (trailerAssets.Count != trailerIds.Count)
                throw new InvalidOperationException("One or more selected trailers were not found.");

            // Ensure all trailer assets belong to the current owner.
            if (trailerAssets.Any(x => x.OwnerUserId != command.OwnerUserId))
                throw new InvalidOperationException("One or more selected trailers do not belong to this owner.");

            // Ensure only trailer-type assets are assigned.
            if (trailerAssets.Any(x => x.Kind != AssetKind.Trailer))
                throw new InvalidOperationException("Only trailer assets can be assigned to a job.");
        }

        // Retrieve the next sort order value for this owner's jobs.
        var nextOrder = await _jobRepository.GetNextSortOrderAsync(command.OwnerUserId);

        // Create the Job domain entity.
        var job = new Job(
            jobId: command.JobId,
            ownerUserId: command.OwnerUserId,
            customerId: command.CustomerId,

            clientCompanyName: command.ClientCompanyName,
            clientContactName: command.ClientContactName,
            clientEmail: command.ClientEmail,
            clientAddressLine1: command.ClientAddressLine1,
            clientCity: command.ClientCity,
            clientCountry: command.ClientCountry,

            pickupCompany: command.PickupCompany,
            pickupAddress: command.PickupAddress,
            deliveryCompany: command.DeliveryCompany,
            deliveryAddress: command.DeliveryAddress,

            referenceNumber: command.ReferenceNumber,
            loadDescription: command.LoadDescription,
            invoiceNumber: command.InvoiceNumber,

            rateType: command.RateType,
            rateValue: command.RateValue,
            quantity: command.Quantity,

            sortOrder: nextOrder,
            driverId: command.DriverId,
            vehicleAssetId: command.VehicleAssetId
        );

        // Apply optional sub-user assignment.
        job.AssignToSubUser(command.AssignedToUserId);

        // Apply trailer assignments.
        job.SetTrailers(trailerIds);

        // Persist the new job.
        await _jobRepository.AddAsync(job);
    }
}