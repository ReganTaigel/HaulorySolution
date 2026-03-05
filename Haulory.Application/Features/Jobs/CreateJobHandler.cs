using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Jobs;

public class CreateJobHandler
{
    private readonly IJobRepository _jobRepository;

    public CreateJobHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    public async Task HandleAsync(CreateJobCommand command)
    {
        var nextOrder = await _jobRepository.GetNextSortOrderAsync(command.OwnerUserId);

        var invoiceNumber = Guid.NewGuid().ToString("N")[..8];

        var job = new Job(
            jobId: command.JobId,
            ownerUserId: command.OwnerUserId,

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
            invoiceNumber: invoiceNumber,

            rateType: command.RateType,
            rateValue: command.RateValue,
            quantity: command.Quantity,

            sortOrder: nextOrder,
            driverId: command.DriverId,
            vehicleAssetId: command.VehicleAssetId
        );

        // persist trailer assignments (0..N)
        job.SetTrailers(command.TrailerAssetIds);

        await _jobRepository.AddAsync(job);
    }
}