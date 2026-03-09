using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Jobs;

public record CreateJobCommand(
    Guid OwnerUserId,
    Guid JobId,

    string ClientCompanyName,
    string? ClientContactName,
    string? ClientEmail,
    string ClientAddressLine1,
    string ClientCity,
    string ClientCountry,

    string PickupCompany,
    string PickupAddress,

    string DeliveryCompany,
    string DeliveryAddress,

    string ReferenceNumber,
    string LoadDescription,

    RateType RateType,
    decimal RateValue,
    decimal Quantity,

    Guid? DriverId,
    Guid? VehicleAssetId,
    Guid? AssignedToUserId,

    IReadOnlyList<Guid>? TrailerAssetIds
);