using Haulory.Contracts.Jobs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Haulory.Api.Jobs;

public sealed class JobRequestValidator
{
    public List<Guid> ValidateCreate(CreateJobRequest request, ModelStateDictionary modelState)
    {
        ValidateShared(
            request.ClientCompanyName,
            request.ClientAddressLine1,
            request.ClientCity,
            request.ClientCountry,
            request.PickupCompany,
            request.PickupAddress,
            request.DeliveryCompany,
            request.DeliveryAddress,
            request.RateValue,
            request.Quantity,
            modelState);
        return ValidateTrailerIds(request.TrailerAssetIds, modelState);
    }

    public List<Guid> ValidateUpdate(UpdateJobRequest request, ModelStateDictionary modelState)
    {
        ValidateShared(
            request.ClientCompanyName,
            request.ClientAddressLine1,
            request.ClientCity,
            request.ClientCountry,
            request.PickupCompany,
            request.PickupAddress,
            request.DeliveryCompany,
            request.DeliveryAddress,
            request.RateValue,
            request.Quantity,
            modelState);

        if (request.WaitTimeMinutes.HasValue && request.WaitTimeMinutes.Value < 0)
            modelState.AddModelError(nameof(request.WaitTimeMinutes), "Wait time cannot be negative.");

        return ValidateTrailerIds(request.TrailerAssetIds, modelState);
    }

    public void ValidateComplete(CompleteJobRequest request, ModelStateDictionary modelState)
    {
        if (string.IsNullOrWhiteSpace(request.ReceiverName))
            modelState.AddModelError(nameof(request.ReceiverName), "Receiver name is required.");

        if (string.IsNullOrWhiteSpace(request.SignatureJson))
            modelState.AddModelError(nameof(request.SignatureJson), "Signature is required.");

        if (request.WaitTimeMinutes.HasValue && request.WaitTimeMinutes.Value < 0)
            modelState.AddModelError(nameof(request.WaitTimeMinutes), "Wait time cannot be negative.");
    }

    public void ValidatePickup(UpdatePickupDetailsRequest request, ModelStateDictionary modelState)
    {
        if (request.WaitTimeMinutes.HasValue && request.WaitTimeMinutes.Value < 0)
            modelState.AddModelError(nameof(request.WaitTimeMinutes), "Wait time cannot be negative.");
    }

    public void ValidateReview(ReviewJobRequest request, ModelStateDictionary modelState)
    {
        if (request.WaitTimeMinutes.HasValue && request.WaitTimeMinutes.Value < 0)
            modelState.AddModelError(nameof(request.WaitTimeMinutes), "Wait time cannot be negative.");
    }

    private static void ValidateShared(
        string? clientCompanyName,
        string? clientAddressLine1,
        string? clientCity,
        string? clientCountry,
        string? pickupCompany,
        string? pickupAddress,
        string? deliveryCompany,
        string? deliveryAddress,
        decimal rateValue,
        decimal quantity,
        ModelStateDictionary modelState)
    {
        if (string.IsNullOrWhiteSpace(clientCompanyName))
            modelState.AddModelError(nameof(CreateJobRequest.ClientCompanyName), "Client company name is required.");

        if (string.IsNullOrWhiteSpace(clientAddressLine1))
            modelState.AddModelError(nameof(CreateJobRequest.ClientAddressLine1), "Client address is required.");

        if (string.IsNullOrWhiteSpace(clientCity))
            modelState.AddModelError(nameof(CreateJobRequest.ClientCity), "Client city is required.");

        if (string.IsNullOrWhiteSpace(clientCountry))
            modelState.AddModelError(nameof(CreateJobRequest.ClientCountry), "Client country is required.");

        if (string.IsNullOrWhiteSpace(pickupCompany))
            modelState.AddModelError(nameof(CreateJobRequest.PickupCompany), "Pickup company is required.");

        if (string.IsNullOrWhiteSpace(pickupAddress))
            modelState.AddModelError(nameof(CreateJobRequest.PickupAddress), "Pickup address is required.");

        if (string.IsNullOrWhiteSpace(deliveryCompany))
            modelState.AddModelError(nameof(CreateJobRequest.DeliveryCompany), "Delivery company is required.");

        if (string.IsNullOrWhiteSpace(deliveryAddress))
            modelState.AddModelError(nameof(CreateJobRequest.DeliveryAddress), "Delivery address is required.");

        if (rateValue < 0)
            modelState.AddModelError(nameof(CreateJobRequest.RateValue), "Rate value cannot be negative.");

        if (quantity < 0)
            modelState.AddModelError(nameof(CreateJobRequest.Quantity), "Quantity cannot be negative.");
    }

    private static List<Guid> ValidateTrailerIds(List<Guid>? trailerAssetIds, ModelStateDictionary modelState)
    {
        var trailerIds = (trailerAssetIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (trailerIds.Count > 2)
            modelState.AddModelError(nameof(CreateJobRequest.TrailerAssetIds), "A maximum of 2 trailers can be assigned to a job.");

        return trailerIds;
    }
}
