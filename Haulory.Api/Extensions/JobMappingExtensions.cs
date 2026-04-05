using Haulory.Contracts.Jobs;
using Haulory.Domain.Entities;

namespace Haulory.Api.Extensions;

// Provides mapping extensions for converting Job domain entities into DTOs.
public static class JobMappingExtensions
{
    // Maps a Job entity to a JobDto for API responses.
    public static JobDto ToDto(this Job job)
    {
        return new JobDto
        {
            CustomerId = job.CustomerId,
            Id = job.Id,
            OwnerUserId = job.OwnerUserId,
            AssignedToUserId = job.AssignedToUserId,
            SortOrder = job.SortOrder,

            // Core job details.
            ReferenceNumber = job.ReferenceNumber,
            LoadDescription = job.LoadDescription,

            // Client details.
            ClientCompanyName = job.ClientCompanyName,
            ClientContactName = job.ClientContactName,
            ClientEmail = job.ClientEmail,
            ClientAddressLine1 = job.ClientAddressLine1,
            ClientCity = job.ClientCity,
            ClientCountry = job.ClientCountry,

            // Pickup details.
            PickupCompany = job.PickupCompany,
            PickupAddress = job.PickupAddress,

            // Delivery details.
            DeliveryCompany = job.DeliveryCompany,
            DeliveryAddress = job.DeliveryAddress,

            // Invoice information.
            InvoiceNumber = job.InvoiceNumber,

            // Pricing and calculation data.
            RateType = job.RateType.ToString(),
            RateValue = job.RateValue,
            Quantity = job.Quantity,
            Total = job.Total,

            // Convert enum status to string for API consumers.
            Status = job.Status.ToString(),

            // Resource assignments.
            DriverId = job.DriverId,
            VehicleAssetId = job.VehicleAssetId,

            // Trailer assignments ordered by position for consistency.
            TrailerAssetIds = job.TrailerAssignments
                .OrderBy(t => t.Position)
                .Select(t => t.TrailerAssetId)
                .ToList(),

            // Delivery outcome details.
            ReceiverName = job.ReceiverName,
            DeliveredAtUtc = job.DeliveredAtUtc,
            DeliverySignatureJson = job.DeliverySignatureJson,

            // Operational notes and flags.
            WaitTimeMinutes = job.WaitTimeMinutes,
            DamageNotes = job.DamageNotes,
            RequiresReview = job.RequiresReview
        };
    }
}