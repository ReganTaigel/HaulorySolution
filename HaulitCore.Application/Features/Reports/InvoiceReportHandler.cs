using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Interfaces.Services;

namespace HaulitCore.Application.Features.Reports;

// Handles the use case of generating an invoice report DTO.
// Combines delivery receipt data (snapshot) with supplier (user account) details.
public class InvoiceReportHandler
{
    // Repository for accessing delivery receipt data.
    private readonly IDeliveryReceiptRepository _receipts;

    // Repository for accessing supplier (user account/business) details.
    private readonly IUserAccountRepository _users;

    private readonly IDocumentSettingsRepository _documentSettings;
    private readonly IInvoiceCalculationService _invoiceCalculationService;

    // Constructor injection of dependencies.
    public InvoiceReportHandler(
        IDeliveryReceiptRepository receipts,
        IUserAccountRepository users,
        IDocumentSettingsRepository documentSettings,
        IInvoiceCalculationService invoiceCalculationService)
    {
        _receipts = receipts;
        _users = users;
        _documentSettings = documentSettings;
        _invoiceCalculationService = invoiceCalculationService;
    }

    // Builds an InvoiceReportDto for a given receipt.
    public async Task<InvoiceReportDto> HandleAsync(
        Guid ownerUserId,
        Guid receiptId)
    {
        // Retrieve the delivery receipt snapshot.
        var r = await _receipts.GetByIdAsync(ownerUserId, receiptId);
        if (r == null)
            throw new InvalidOperationException("Receipt not found.");

        // Retrieve supplier (business) details from the user account.
        var u = await _users.GetByIdAsync(ownerUserId);
        if (u == null)
            throw new InvalidOperationException("User account not found.");

        var settings = await _documentSettings.GetOrCreateAsync(ownerUserId);
        var calculation = _invoiceCalculationService.Calculate(
            rateValue: r.RateValue,
            quantity: r.Quantity,
            gstEnabled: settings.GstEnabled,
            gstRatePercent: settings.GstRatePercent,
            fuelSurchargeEnabled: settings.FuelSurchargeEnabled,
            fuelSurchargePercent: settings.FuelSurchargePercent,
            waitTimeChargeEnabled: settings.WaitTimeChargeEnabled,
            waitTimeChargeAmount: settings.WaitTimeCharge,
            handUnloadChargeEnabled: settings.HandUnloadChargeEnabled,
            handUnloadChargeAmount: settings.HandUnloadCharge);

        // Construct and return the invoice report DTO.
        return new InvoiceReportDto
        {
            // Id's
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

            // Calculated totals from current settings.
            Subtotal = calculation.Subtotal,

            // Fuel surcharge configuration and values.
            FuelSurchargeEnabled = settings.FuelSurchargeEnabled,
            FuelSurchargePercent = settings.FuelSurchargePercent,
            FuelSurchargeAmount = calculation.FuelSurchargeAmount,

            // Wait time configuration and values.
            WaitTimeChargeEnabled = settings.WaitTimeChargeEnabled,
            WaitTimeChargeAmount = calculation.WaitTimeChargeAmount,

            // Hand unload configuration and values.
            HandUnloadChargeEnabled = settings.HandUnloadChargeEnabled,
            HandUnloadChargeAmount = calculation.HandUnloadChargeAmount,

            // GST configuration and values.
            GstEnabled = settings.GstEnabled,
            GstRatePercent = settings.GstRatePercent,
            GstAmount = calculation.GstAmount,

            // Final total amount.
            Total = calculation.Total
        };
    }
}