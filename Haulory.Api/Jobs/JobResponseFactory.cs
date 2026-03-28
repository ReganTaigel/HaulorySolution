using Haulory.Contracts.Jobs;
using Haulory.Domain.Entities;

namespace Haulory.Api.Jobs;

public sealed class JobResponseFactory
{
    public JobDto ToDto(Job job)
        => new()
        {
            CustomerId = job.CustomerId,
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

    public object ToCreated(Job job)
        => new
        {
            message = "Job created.",
            jobId = job.Id,
            referenceNumber = job.ReferenceNumber,
            invoiceNumber = job.InvoiceNumber,
            status = job.Status.ToString()
        };

    public object ToUpdated(Job job)
        => new
        {
            message = "Job updated.",
            jobId = job.Id,
            referenceNumber = job.ReferenceNumber,
            invoiceNumber = job.InvoiceNumber,
            status = job.Status.ToString()
        };

    public object ToPickupUpdated(Job job)
        => new
        {
            message = "Pickup details updated.",
            jobId = job.Id,
            waitTimeMinutes = job.WaitTimeMinutes,
            damageNotes = job.DamageNotes
        };

    public object ToReviewed(Job job)
        => new
        {
            message = "Job reviewed and approved.",
            jobId = job.Id,
            status = job.Status.ToString(),
            waitTimeMinutes = job.WaitTimeMinutes,
            damageNotes = job.DamageNotes
        };

    public object ToCompleted(Job job, Guid? receiptId)
        => new
        {
            message = job.Status == Haulory.Domain.Enums.JobStatus.DeliveredPendingReview
                ? "Job delivered and sent for review."
                : "Job completed.",
            jobId = job.Id,
            status = job.Status.ToString(),
            requiresReview = job.RequiresReview,
            receiptId
        };
}
