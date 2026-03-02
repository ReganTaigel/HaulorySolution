namespace Haulory.Application.Features.Reports;

public class PodReportDto
{
    public Guid ReceiptId { get; set; }
    public Guid JobId { get; set; }

    // Supplier snapshot (business)
    public string SupplierBusinessName { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierAddressLine1 { get; set; } = string.Empty;
    public string SupplierCity { get; set; } = string.Empty;
    public string SupplierCountry { get; set; } = string.Empty;
    public string? SupplierGstNumber { get; set; }
    public string? SupplierNzbn { get; set; }

    public string ReferenceNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;
    public DateTime DeliveredAtUtc { get; set; }

    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string LoadDescription { get; set; } = string.Empty;

    public string ReceiverName { get; set; } = string.Empty;
    public string SignatureJson { get; set; } = string.Empty;
}