using Haulory.Domain.Enums;

namespace Haulory.Application.Features.Jobs;

public record CreateJobCommand(
    string PickupCompany,
    string PickupAddress,
    string DeliveryCompany,
    string DeliveryAddress,
    string ReferenceNumber,
    string LoadDescription,
    RateType RateType,
    decimal RateValue,
    int Quantity);
