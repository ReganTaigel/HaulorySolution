using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Jobs;

public class CreateJobHandler
{
    private readonly IJobRepository _jobRepository;
    private readonly IVehicleAssetRepository _vehicleAssetRepository;

    public CreateJobHandler(
        IJobRepository jobRepository,
        IVehicleAssetRepository vehicleAssetRepository)
    {
        _jobRepository = jobRepository;
        _vehicleAssetRepository = vehicleAssetRepository;
    }

    public async Task HandleAsync(CreateJobCommand command)
    {
        if (command.OwnerUserId == Guid.Empty)
            throw new InvalidOperationException("Owner user is required.");

        var trailerIds = command.TrailerAssetIds?
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList() ?? new List<Guid>();

        if (trailerIds.Count > 2)
            throw new InvalidOperationException("A maximum of 2 trailers can be assigned to a job.");

        if (trailerIds.Count > 0)
        {
            var trailerAssets = await _vehicleAssetRepository.GetByIdsAsync(trailerIds);

            if (trailerAssets.Count != trailerIds.Count)
                throw new InvalidOperationException("One or more selected trailers were not found.");

            if (trailerAssets.Any(x => x.OwnerUserId != command.OwnerUserId))
                throw new InvalidOperationException("One or more selected trailers do not belong to this owner.");

            if (trailerAssets.Any(x => x.Kind != AssetKind.Trailer))
                throw new InvalidOperationException("Only trailer assets can be assigned to a job.");
        }

        var nextOrder = await _jobRepository.GetNextSortOrderAsync(command.OwnerUserId);

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

        job.AssignToSubUser(command.AssignedToUserId);
        job.SetTrailers(trailerIds);

        await _jobRepository.AddAsync(job);
    }
}