using HaulitCore.Application.Interfaces.Repositories;

namespace HaulitCore.Application.Features.Reports;

// Handles the use case of generating a Proof of Delivery (POD) report DTO.
// Combines delivery receipt snapshot data with supplier (user account) details.
public class PodReportHandler
{
    // Repository for accessing delivery receipt data.
    private readonly IDeliveryReceiptRepository _receipts;

    // Repository for accessing supplier (user account/business) details.
    private readonly IUserAccountRepository _users;

    // Constructor injection of dependencies.
    public PodReportHandler(
        IDeliveryReceiptRepository receipts,
        IUserAccountRepository users)
    {
        _receipts = receipts;
        _users = users;
    }

    // Builds a PodReportDto for a given receipt.
    public async Task<PodReportDto> HandleAsync(Guid ownerUserId, Guid receiptId)
    {
        // Retrieve the delivery receipt snapshot.
        var r = await _receipts.GetByIdAsync(ownerUserId, receiptId);
        if (r == null)
            throw new InvalidOperationException("Receipt not found.");

        // Retrieve supplier (business) details from the user account.
        var u = await _users.GetByIdAsync(ownerUserId);
        if (u == null)
            throw new InvalidOperationException("User account not found.");

        // Debug logging for troubleshooting POD data issues.
        System.Diagnostics.Debug.WriteLine(
            $"[PodReportHandler] ReceiptId={r.Id}, JobId={r.JobId}, DamageNotes='{r.DamageNotes}', WaitTimeMinutes={r.WaitTimeMinutes}");

        // Construct and return the POD report DTO.
        return new PodReportDto
        {
            ReceiptId = r.Id,
            JobId = r.JobId,

            // Supplier (your business issuing the POD).
            SupplierBusinessName = u.BusinessName,
            SupplierEmail = u.BusinessEmail ?? u.Email,
            SupplierAddressLine1 = u.BusinessAddress1 ?? string.Empty,
            SupplierCity = u.BusinessCity ?? string.Empty,
            SupplierCountry = u.BusinessCountry ?? "New Zealand",
            SupplierGstNumber = u.SupplierGstNumber,
            SupplierNzbn = u.SupplierNzbn,

            // Reference and delivery details.
            ReferenceNumber = r.ReferenceNumber,
            InvoiceNumber = r.InvoiceNumber,
            DeliveredAtUtc = r.DeliveredAtUtc,

            // Pickup and delivery locations.
            PickupCompany = r.PickupCompany,
            PickupAddress = r.PickupAddress,
            DeliveryCompany = r.DeliveryCompany,
            DeliveryAddress = r.DeliveryAddress,
            LoadDescription = r.LoadDescription,

            // Receiver confirmation details.
            ReceiverName = r.ReceiverName ?? string.Empty,

            // Signature data (fallback to empty string if null).
            SignatureJson = r.SignatureJson ?? string.Empty,

            // Optional delivery notes.
            DamageNotes = r.DamageNotes,
            WaitTimeMinutes = r.WaitTimeMinutes
        };
    }
}