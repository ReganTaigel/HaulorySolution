using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Jobs;

#region Command: Create Job

public record CreateJobCommand(
    // Ownership
    Guid OwnerUserId,
    Guid JobId,

    // Client (Bill To)
    string ClientCompanyName,
    string? ClientContactName,
    string? ClientEmail,
    string ClientAddressLine1,
    string ClientCity,
    string ClientCountry,

    // Pickup
    string PickupCompany,
    string PickupAddress,

    // Delivery
    string DeliveryCompany,
    string DeliveryAddress,

    // Job
    string ReferenceNumber,
    string LoadDescription,

    // Pricing
    RateType RateType,
    decimal RateValue,
    decimal Quantity,

    // Allocation (optional)
    Guid? DriverId,
    Guid? VehicleAssetId
);

#endregion