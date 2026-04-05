using Haulory.Contracts.Reports;
using Haulory.Domain.Entities;

namespace Haulory.Api.Extensions;

// Provides mapping extensions for converting DeliveryReceipt domain entities into DTOs.
public static class DeliveryReceiptMappingExtensions
{
    // Maps a DeliveryReceipt entity to a DeliveryReceiptDto for API responses.
    public static DeliveryReceiptDto ToDto(this DeliveryReceipt receipt)
    {
        return new DeliveryReceiptDto
        {
            Id = receipt.Id,
            JobId = receipt.JobId,
            ReferenceNumber = receipt.ReferenceNumber,
            InvoiceNumber = receipt.InvoiceNumber,

            // Pickup and delivery details.
            PickupCompany = receipt.PickupCompany,
            PickupAddress = receipt.PickupAddress,
            DeliveryCompany = receipt.DeliveryCompany,
            DeliveryAddress = receipt.DeliveryAddress,

            // Job/load information.
            LoadDescription = receipt.LoadDescription,
            ReceiverName = receipt.ReceiverName,
            DeliveredAtUtc = receipt.DeliveredAtUtc,

            // Pricing and calculation details.
            RateType = receipt.RateType.ToString(),
            RateValue = receipt.RateValue,
            Quantity = receipt.Quantity,
            Total = receipt.Total,

            // GST configuration and values.
            GstEnabled = receipt.GstEnabled,
            GstRatePercent = receipt.GstRatePercent,

            // Fuel surcharge configuration and values.
            FuelSurchargeEnabled = receipt.FuelSurchargeEnabled,
            FuelSurchargePercent = receipt.FuelSurchargePercent,
            FuelSurchargeAmount = receipt.FuelSurchargeAmount,

            // Operational notes.
            DamageNotes = receipt.DamageNotes,
            WaitTimeMinutes = receipt.WaitTimeMinutes,

            // Display flags for POD (Proof of Delivery) documents.
            ShowDamageNotesOnPod = receipt.ShowDamageNotesOnPod,
            ShowWaitTimeOnPod = receipt.ShowWaitTimeOnPod
        };
    }
}