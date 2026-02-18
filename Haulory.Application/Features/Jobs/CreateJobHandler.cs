using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Application.Features.Jobs;

public class CreateJobHandler
{
    #region Dependencies

    private readonly IJobRepository _jobRepository;

    #endregion

    #region Constructor

    public CreateJobHandler(IJobRepository jobRepository)
    {
        _jobRepository = jobRepository;
    }

    #endregion

    #region Public API


    // Creates and persists a new Job.
    // Responsibilities:
    // - Determine next sort order (for list ordering)
    // - Create Job aggregate
    // - Persist via repository
 
    public async Task HandleAsync(CreateJobCommand command)
    {
        // Determine ordering position for the new job (UI + reporting)
        var nextOrder = await _jobRepository.GetNextSortOrderAsync();

        // Create a short invoice number token (8 chars).
        // Note: This is not guaranteed unique; repository/database should enforce uniqueness if required.
        var invoiceNumber = Guid.NewGuid().ToString("N")[..8];

        // Create the Job aggregate. Allocation (driver/vehicle) is optional at creation time.
        var job = new Job(
            command.OwnerUserId,
            command.PickupCompany,
            command.PickupAddress,
            command.DeliveryCompany,
            command.DeliveryAddress,
            command.ReferenceNumber,
            command.LoadDescription,
            invoiceNumber: invoiceNumber,
            command.RateType,
            command.RateValue,
            command.Quantity,
            nextOrder,
            driverId: command.DriverId,
            vehicleAssetId: command.VehicleAssetId
        );

        // Persist the new job
        await _jobRepository.AddAsync(job);
    }

    #endregion
}
