using Haulory.Application.Interfaces.Repositories;

namespace Haulory.Application.Features.Reports;

// Handles the use case of generating an invoice report DTO.
// Combines delivery receipt data (snapshot) with supplier (user account) details.
public class InvoiceReportHandler
{
    // Repository for accessing delivery receipt data.
    private readonly IDeliveryReceiptRepository _receipts;

    // Repository for accessing supplier (user account/business) details.
    private readonly IUserAccountRepository _users;

    // Constructor injection of dependencies.
    public InvoiceReportHandler(
        IDeliveryReceiptRepository receipts,
        IUserAccountRepository users)
    {
        _receipts = receipts;
        _users = users;
    }

    // Builds an InvoiceReportDto for a given receipt.
    public async Task<InvoiceReportDto> HandleAsync(
        Guid ownerUserId,
        Guid receiptId,
        bool includeGst,
        decimal gstRate = 0.15m)
    {
        // Retrieve the delivery receipt snapshot.
        var r = await _receipts.GetByIdAsync(ownerUserId, receiptId);
        if (r == null)
            throw new InvalidOperationException("Receipt not found.");

        // Retrieve supplier (business) details from the user account.
        var u = await _users.GetByIdAsync(ownerUserId);
        if (u == null)
            throw new InvalidOperationException("User account not found.");

        // Construct and return the invoice report DTO.
        return new InvoiceReportDto
        {
            ReceiptId = r.Id,
            JobId = r.JobId,

            // Invoice identifiers and delivery timestamp.
            InvoiceNumber = r.InvoiceNumber,
            ReferenceNumber = r.ReferenceNumber,
            DeliveredAtUtc = r.DeliveredAtUtc,

            // Supplier (your business issuing the invoice).
            SupplierBusinessName = u.BusinessName,
            SupplierEmail = u.BusinessEmail ?? u.Email,
            SupplierAddressLine1 = u.BusinessAddress1 ?? string.Empty,
            SupplierCity = u.BusinessCity ?? string.Empty,
            SupplierCountry = u.BusinessCountry ?? "New Zealand",
            SupplierGstNumber = u.SupplierGstNumber,
            SupplierNzbn = u.SupplierNzbn,

            // Client (bill-to party) details from receipt snapshot.
            ClientCompanyName = r.ClientCompanyName,
            ClientContactName = r.ClientContactName,
            ClientEmail = r.ClientEmail,
            ClientAddressLine1 = r.ClientAddressLine1,
            ClientCity = r.ClientCity,
            ClientCountry = r.ClientCountry,

            // Pricing details from receipt snapshot.
            RateTypeDisplay = r.RateType.ToString(),
            RateValue = r.RateValue,
            Quantity = r.Quantity,

            // Calculated totals (already stored in receipt).
            Subtotal = r.Subtotal,

            // Fuel surcharge configuration and values.
            FuelSurchargeEnabled = r.FuelSurchargeEnabled,
            FuelSurchargePercent = r.FuelSurchargePercent,
            FuelSurchargeAmount = r.FuelSurchargeAmount,

            // GST configuration and values.
            GstEnabled = r.GstEnabled,
            GstRatePercent = r.GstRatePercent,
            GstAmount = r.GstAmount,

            // Final total amount.
            Total = r.Total
        };
    }
}