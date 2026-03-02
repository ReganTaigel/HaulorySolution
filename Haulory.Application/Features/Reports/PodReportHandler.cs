using Haulory.Application.Interfaces.Repositories;

namespace Haulory.Application.Features.Reports;

public class PodReportHandler
{
    private readonly IDeliveryReceiptRepository _receipts;
    private readonly IUserAccountRepository _users;

    public PodReportHandler(
        IDeliveryReceiptRepository receipts,
        IUserAccountRepository users)
    {
        _receipts = receipts;
        _users = users;
    }

    public async Task<PodReportDto> HandleAsync(Guid ownerUserId, Guid receiptId)
    {
        if (ownerUserId == Guid.Empty)
            throw new ArgumentException("ownerUserId required.");

        var r = await _receipts.GetByIdAsync(ownerUserId, receiptId);
        if (r == null)
            throw new InvalidOperationException("Receipt not found.");

        var u = await _users.GetByIdAsync(ownerUserId);
        if (u == null)
            throw new InvalidOperationException("Owner user not found.");

        return new PodReportDto
        {
            ReceiptId = r.Id,
            JobId = r.JobId,

            // Supplier = main user profile
            SupplierBusinessName = u.BusinessName,
            SupplierEmail = u.BusinessEmail ?? u.Email,
            SupplierAddressLine1 = u.BusinessAddress1 ?? "",
            SupplierCity = u.BusinessCity ?? "",
            SupplierCountry = u.BusinessCountry ?? "New Zealand",
            SupplierGstNumber = u.SupplierGstNumber,
            SupplierNzbn = u.SupplierNzbn,

            // Receipt snapshot
            ReferenceNumber = r.ReferenceNumber,
            InvoiceNumber = r.InvoiceNumber,
            DeliveredAtUtc = r.DeliveredAtUtc,

            PickupCompany = r.PickupCompany,
            PickupAddress = r.PickupAddress,
            DeliveryCompany = r.DeliveryCompany,
            DeliveryAddress = r.DeliveryAddress,
            LoadDescription = r.LoadDescription,

            ReceiverName = r.ReceiverName,
            SignatureJson = r.SignatureJson
        };
    }
}