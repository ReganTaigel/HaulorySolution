using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Jobs;

public class CreateJobHandler
{
    private readonly IJobRepository _repository;

    public CreateJobHandler(IJobRepository repository)
    {
        _repository = repository;
    }

    public async Task HandleAsync(CreateJobCommand command)
    {
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
            command.Quantity);

        await _repository.AddAsync(job);
    }
}
