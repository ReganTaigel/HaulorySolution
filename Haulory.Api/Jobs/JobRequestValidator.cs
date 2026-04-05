using Haulory.Contracts.Jobs;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Haulory.Api.Jobs;

// Handles validation logic for job-related requests.
// Uses ModelState to accumulate validation errors instead of throwing exceptions.
public sealed class JobRequestValidator
{
    // Validates a CreateJobRequest and returns cleaned trailer IDs.
    public List<Guid> ValidateCreate(CreateJobRequest request, ModelStateDictionary modelState)
    {
        // Validate all shared fields required for job creation.
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

        // Validate and normalise trailer IDs.
        return ValidateTrailerIds(request.TrailerAssetIds, modelState);
    }

    // Validates an UpdateJobRequest and returns cleaned trailer IDs.
    public List<Guid> ValidateUpdate(UpdateJobRequest request, ModelStateDictionary modelState)
    {
        // Reuse shared validation rules.
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

        // Ensure wait time is not negative.
        if (request.WaitTimeMinutes.HasValue && request.WaitTimeMinutes.Value < 0)
            modelState.AddModelError(nameof(request.WaitTimeMinutes), "Wait time cannot be negative.");

        // Validate and normalise trailer IDs.
        return ValidateTrailerIds(request.TrailerAssetIds, modelState);
    }

    // Validates a CompleteJobRequest (delivery completion stage).
    public void ValidateComplete(CompleteJobRequest request, ModelStateDictionary modelState)
    {
        // Receiver name must be provided.
        if (string.IsNullOrWhiteSpace(request.ReceiverName))
            modelState.AddModelError(nameof(request.ReceiverName), "Receiver name is required.");

        // Signature must be provided (used for POD).
        if (string.IsNullOrWhiteSpace(request.SignatureJson))
            modelState.AddModelError(nameof(request.SignatureJson), "Signature is required.");

        // Ensure wait time is not negative.
        if (request.WaitTimeMinutes.HasValue && request.WaitTimeMinutes.Value < 0)
            modelState.AddModelError(nameof(request.WaitTimeMinutes), "Wait time cannot be negative.");
    }

    // Validates pickup-related updates.
    public void ValidatePickup(UpdatePickupDetailsRequest request, ModelStateDictionary modelState)
    {
        // Ensure wait time is not negative.
        if (request.WaitTimeMinutes.HasValue && request.WaitTimeMinutes.Value < 0)
            modelState.AddModelError(nameof(request.WaitTimeMinutes), "Wait time cannot be negative.");
    }

    // Validates review stage updates.
    public void ValidateReview(ReviewJobRequest request, ModelStateDictionary modelState)
    {
        // Ensure wait time is not negative.
        if (request.WaitTimeMinutes.HasValue && request.WaitTimeMinutes.Value < 0)
            modelState.AddModelError(nameof(request.WaitTimeMinutes), "Wait time cannot be negative.");
    }

    // Shared validation logic used by both create and update operations.
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
        // Validate client details.
        if (string.IsNullOrWhiteSpace(clientCompanyName))
            modelState.AddModelError(nameof(CreateJobRequest.ClientCompanyName), "Client company name is required.");

        if (string.IsNullOrWhiteSpace(clientAddressLine1))
            modelState.AddModelError(nameof(CreateJobRequest.ClientAddressLine1), "Client address is required.");

        if (string.IsNullOrWhiteSpace(clientCity))
            modelState.AddModelError(nameof(CreateJobRequest.ClientCity), "Client city is required.");

        if (string.IsNullOrWhiteSpace(clientCountry))
            modelState.AddModelError(nameof(CreateJobRequest.ClientCountry), "Client country is required.");

        // Validate pickup details.
        if (string.IsNullOrWhiteSpace(pickupCompany))
            modelState.AddModelError(nameof(CreateJobRequest.PickupCompany), "Pickup company is required.");

        if (string.IsNullOrWhiteSpace(pickupAddress))
            modelState.AddModelError(nameof(CreateJobRequest.PickupAddress), "Pickup address is required.");

        // Validate delivery details.
        if (string.IsNullOrWhiteSpace(deliveryCompany))
            modelState.AddModelError(nameof(CreateJobRequest.DeliveryCompany), "Delivery company is required.");

        if (string.IsNullOrWhiteSpace(deliveryAddress))
            modelState.AddModelError(nameof(CreateJobRequest.DeliveryAddress), "Delivery address is required.");

        // Ensure rate and quantity are not negative.
        if (rateValue < 0)
            modelState.AddModelError(nameof(CreateJobRequest.RateValue), "Rate value cannot be negative.");

        if (quantity < 0)
            modelState.AddModelError(nameof(CreateJobRequest.Quantity), "Quantity cannot be negative.");
    }

    // Validates trailer assignments and enforces business rules.
    private static List<Guid> ValidateTrailerIds(List<Guid>? trailerAssetIds, ModelStateDictionary modelState)
    {
        // Remove empty GUIDs and duplicates.
        var trailerIds = (trailerAssetIds ?? new List<Guid>())
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        // Enforce maximum of 2 trailers per job.
        if (trailerIds.Count > 2)
            modelState.AddModelError(nameof(CreateJobRequest.TrailerAssetIds), "A maximum of 2 trailers can be assigned to a job.");

        return trailerIds;
    }
}