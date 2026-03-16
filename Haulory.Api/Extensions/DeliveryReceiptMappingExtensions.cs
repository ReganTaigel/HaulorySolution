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

            ReferenceNumber = receipt.ReferenceNumber ?? string.Empty,
            InvoiceNumber = receipt.InvoiceNumber ?? string.Empty,

            PickupCompany = receipt.PickupCompany ?? string.Empty,
            PickupAddress = receipt.PickupAddress ?? string.Empty,

            DeliveryCompany = receipt.DeliveryCompany ?? string.Empty,
            DeliveryAddress = receipt.DeliveryAddress ?? string.Empty,

            LoadDescription = receipt.LoadDescription ?? string.Empty,

            ReceiverName = receipt.ReceiverName,
            DeliveredAtUtc = receipt.DeliveredAtUtc,

            RateType = receipt.RateType.ToString(),
            RateValue = receipt.RateValue,
            Quantity = receipt.Quantity,
            Total = receipt.Total
        };
    }
}