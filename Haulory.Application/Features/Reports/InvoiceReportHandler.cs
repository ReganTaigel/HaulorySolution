using Haulory.Application.Interfaces.Repositories;

namespace Haulory.Application.Features.Reports;

public class InvoiceReportHandler
{
    private readonly IDeliveryReceiptRepository _receipts;
    private readonly IUserAccountRepository _users;

    public InvoiceReportHandler(
        IDeliveryReceiptRepository receipts,
        IUserAccountRepository users)
    {
        _receipts = receipts;
        _users = users;
    }

    public async Task<InvoiceReportDto> HandleAsync(
        Guid ownerUserId,
        Guid receiptId,
        bool includeGst,
        decimal gstRate = 0.15m)
    {
        var r = await _receipts.GetByIdAsync(ownerUserId, receiptId);
        if (r == null)
            throw new InvalidOperationException("Receipt not found.");

        var u = await _users.GetByIdAsync(ownerUserId);
        if (u == null)
            throw new InvalidOperationException("User account not found.");

        return new InvoiceReportDto
        {
            ReceiptId = r.Id,
            JobId = r.JobId,

            InvoiceNumber = r.InvoiceNumber,
            ReferenceNumber = r.ReferenceNumber,
            DeliveredAtUtc = r.DeliveredAtUtc,

            SupplierBusinessName = u.BusinessName,
            SupplierEmail = u.BusinessEmail ?? u.Email,
            SupplierAddressLine1 = u.BusinessAddress1 ?? string.Empty,
            SupplierCity = u.BusinessCity ?? string.Empty,
            SupplierCountry = u.BusinessCountry ?? "New Zealand",
            SupplierGstNumber = u.SupplierGstNumber,
            SupplierNzbn = u.SupplierNzbn,

            ClientCompanyName = r.ClientCompanyName,
            ClientContactName = r.ClientContactName,
            ClientEmail = r.ClientEmail,
            ClientAddressLine1 = r.ClientAddressLine1,
            ClientCity = r.ClientCity,
            ClientCountry = r.ClientCountry,

            RateTypeDisplay = r.RateType.ToString(),
            RateValue = r.RateValue,
            Quantity = r.Quantity,

            Subtotal = r.Subtotal,

            FuelSurchargeEnabled = r.FuelSurchargeEnabled,
            FuelSurchargePercent = r.FuelSurchargePercent,
            FuelSurchargeAmount = r.FuelSurchargeAmount,

            GstEnabled = r.GstEnabled,
            GstRatePercent = r.GstRatePercent,
            GstAmount = r.GstAmount,

            Total = r.Total
        };
    }
}