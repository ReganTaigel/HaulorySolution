using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Jobs;

public sealed record CreateJobCommand(
    Guid OwnerUserId,
    Guid? CustomerId,
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
    string InvoiceNumber,
    RateType RateType,
    decimal Quantity,
    decimal RateValue,
    Guid? DriverId,
    Guid? VehicleAssetId,
    Guid? AssignedToUserId,
    List<Guid>? TrailerAssetIds);