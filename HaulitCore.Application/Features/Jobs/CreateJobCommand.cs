using HaulitCore.Domain.Enums;

namespace HaulitCore.Application.Features.Jobs;

// Represents the application command used to create a new job.
// Contains all data required to construct a Job domain entity.
public sealed record CreateJobCommand(
    // Owner/business account creating the job.
    Guid OwnerUserId,

    // Optional existing customer reference (can be null for new/matched customers).
    Guid? CustomerId,

    // Unique identifier for the job (generated before handler execution).
    Guid JobId,

    // Client (billing party) details.
    string ClientCompanyName,
    string? ClientContactName,
    string? ClientEmail,
    string ClientAddressLine1,
    string ClientCity,
    string ClientCountry,

    // Pickup location details.
    string PickupCompany,
    string PickupAddress,

    // Delivery location details.
    string DeliveryCompany,
    string DeliveryAddress,

    // Job reference and description.
    string ReferenceNumber,
    string LoadDescription,

    // Pre-generated invoice number (ensured unique before command execution).
    string InvoiceNumber,

    // Pricing configuration.
    RateType RateType,
    decimal Quantity,
    decimal RateValue,

    // Optional resource assignments.
    Guid? DriverId,
    Guid? VehicleAssetId,

    // Optional assignment to a sub-user (e.g., dispatcher/driver app user).
    Guid? AssignedToUserId,

    // Optional trailer asset assignments (typically max 2, enforced elsewhere).
    List<Guid>? TrailerAssetIds
);