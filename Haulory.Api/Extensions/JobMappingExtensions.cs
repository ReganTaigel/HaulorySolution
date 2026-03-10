using Haulory.Api.Contracts.Jobs;
using Haulory.Domain.Entities;

namespace Haulory.Api.Extensions;

public static class JobMappingExtensions
{
    public static JobDto ToDto(this Job job)
    {
        return new JobDto
        {
            Id = job.Id,
            OwnerUserId = job.OwnerUserId,
            AssignedToUserId = job.AssignedToUserId,
            SortOrder = job.SortOrder,

            ReferenceNumber = job.ReferenceNumber,
            LoadDescription = job.LoadDescription,

            ClientCompanyName = job.ClientCompanyName,
            ClientContactName = job.ClientContactName,
            ClientEmail = job.ClientEmail,
            ClientAddressLine1 = job.ClientAddressLine1,
            ClientCity = job.ClientCity,
            ClientCountry = job.ClientCountry,

            PickupCompany = job.PickupCompany,
            PickupAddress = job.PickupAddress,

            DeliveryCompany = job.DeliveryCompany,
            DeliveryAddress = job.DeliveryAddress,

            InvoiceNumber = job.InvoiceNumber,

            RateType = job.RateType.ToString(),
            RateValue = job.RateValue,
            Quantity = job.Quantity,
            Total = job.Total,

            Status = job.Status.ToString(),

            DriverId = job.DriverId,
            VehicleAssetId = job.VehicleAssetId,

            TrailerAssetIds = job.TrailerAssignments
                .OrderBy(t => t.Position)
                .Select(t => t.TrailerAssetId)
                .ToList(),

            ReceiverName = job.ReceiverName,
            DeliveredAtUtc = job.DeliveredAtUtc,
            DeliverySignatureJson = job.DeliverySignatureJson,

            WaitTimeMinutes = job.WaitTimeMinutes,
            DamageNotes = job.DamageNotes,
            RequiresReview = job.RequiresReview
        };
    }
}