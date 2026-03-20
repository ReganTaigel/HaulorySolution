using Haulory.Contracts.Reports;
using Haulory.Domain.Entities;

namespace Haulory.Api.Extensions;

public static class DeliveryReceiptMappingExtensions
{
    public static DeliveryReceiptDto ToDto(this DeliveryReceipt receipt)
    {
        return new DeliveryReceiptDto
        {
            Id = receipt.Id,
            JobId = receipt.JobId,
            ReferenceNumber = receipt.ReferenceNumber,
            InvoiceNumber = receipt.InvoiceNumber,
            PickupCompany = receipt.PickupCompany,
            PickupAddress = receipt.PickupAddress,
            DeliveryCompany = receipt.DeliveryCompany,
            DeliveryAddress = receipt.DeliveryAddress,
            LoadDescription = receipt.LoadDescription,
            ReceiverName = receipt.ReceiverName,
            DeliveredAtUtc = receipt.DeliveredAtUtc,
            RateType = receipt.RateType.ToString(),
            RateValue = receipt.RateValue,
            Quantity = receipt.Quantity,
            Total = receipt.Total,

            GstEnabled = receipt.GstEnabled,
            GstRatePercent = receipt.GstRatePercent,

            FuelSurchargeEnabled = receipt.FuelSurchargeEnabled,
            FuelSurchargePercent = receipt.FuelSurchargePercent,
            FuelSurchargeAmount = receipt.FuelSurchargeAmount,

            DamageNotes = receipt.DamageNotes,
            WaitTimeMinutes = receipt.WaitTimeMinutes,

            ShowDamageNotesOnPod = receipt.ShowDamageNotesOnPod,
            ShowWaitTimeOnPod = receipt.ShowWaitTimeOnPod
        };
    }
}