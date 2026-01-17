using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Jobs;

public class CreateJobHandler
{
    private readonly IJobRepository _jobRepository;

    public CreateJobHandler(IJobRepository jobrepository)
    {
        _jobRepository = jobrepository;
    }

    public async Task HandleAsync(CreateJobCommand command)
    {
        // Determine manual sort order
        var nextOrder = await _jobRepository.GetNextSortOrderAsync();

        var job = new Job(
            command.PickupCompany,
            command.PickupAddress,
            command.DeliveryCompany,
            command.DeliveryAddress,
            command.ReferenceNumber,
            command.LoadDescription,
            invoiceNumber: Guid.NewGuid().ToString("N")[..8],
            command.RateType,
            command.RateValue,
            command.Quantity,
            nextOrder);

        await _jobRepository.AddAsync(job);
    }
}
