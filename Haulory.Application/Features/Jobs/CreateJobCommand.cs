using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Jobs;

#region Command: Create Job

/// <summary>
/// Command used to create a new transport job.
/// 
/// This represents the input data required to persist a Job aggregate.
/// Validation and business rules are handled inside the corresponding handler.
/// </summary>
public record CreateJobCommand(

#region Ownership


    // The owner (customer/company user) creating the job.
    // All jobs are scoped to this OwnerUserId.

    Guid OwnerUserId,

#endregion

#region Pickup Details


    // Name of the pickup company.

    string PickupCompany,

    // Physical pickup address.

    string PickupAddress,

#endregion

#region Delivery Details

    // Name of the delivery company.
    string DeliveryCompany,

    // Physical delivery address.
    string DeliveryAddress,

#endregion

#region Job Information
    // Customer or freight reference number.
    // Used for tracking and invoicing.
    string ReferenceNumber,

    // Description of the load being transported.
    string LoadDescription,

#endregion

#region Pricing

    // Defines how the job is charged (e.g., per hour, per load, per tonne).
    RateType RateType,

    // Monetary rate value associated with the selected RateType.
    decimal RateValue,

    // Quantity used in pricing calculation (e.g., hours, loads, tonnes).
    decimal Quantity,

#endregion

#region Allocation (Optional)

    // Assigned driver (optional at creation time).
    Guid? DriverId,


    // Assigned vehicle or asset (optional at creation time).
    Guid? VehicleAssetId

#endregion
);

#endregion
