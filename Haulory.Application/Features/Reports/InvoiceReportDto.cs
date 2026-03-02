namespace Haulory.Application.Features.Reports;

public class InvoiceReportDto
{
    public Guid ReceiptId { get; set; }
    public Guid JobId { get; set; }

    public string InvoiceNumber { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;

    // Supplier (your business)
    public string SupplierBusinessName { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierAddressLine1 { get; set; } = string.Empty;
    public string SupplierCity { get; set; } = string.Empty;
    public string SupplierCountry { get; set; } = string.Empty;
    public string? SupplierGstNumber { get; set; }
    public string? SupplierNzbn { get; set; }

    // Client (bill-to)
    public string ClientCompanyName { get; set; } = string.Empty;
    public string? ClientContactName { get; set; }
    public string? ClientEmail { get; set; }
    public string ClientAddressLine1 { get; set; } = string.Empty;
    public string ClientCity { get; set; } = string.Empty;
    public string ClientCountry { get; set; } = string.Empty;

    // Pricing snapshot
    public string RateTypeDisplay { get; set; } = string.Empty;
    public decimal RateValue { get; set; }
    public decimal Quantity { get; set; }

    public decimal Subtotal { get; set; }
    public decimal GstAmount { get; set; }
    public decimal Total { get; set; }

    public DateTime DeliveredAtUtc { get; set; }
}