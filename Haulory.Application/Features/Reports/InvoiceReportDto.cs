namespace Haulory.Application.Features.Reports;

// Represents the data required to generate an invoice report (e.g., PDF).
// Acts as a snapshot of billing, supplier, and delivery information.
public class InvoiceReportDto
{
    // Identifiers for traceability.
    public Guid ReceiptId { get; set; }
    public Guid JobId { get; set; }

    // Invoice metadata.
    public string InvoiceNumber { get; set; } = string.Empty;
    public string ReferenceNumber { get; set; } = string.Empty;

    // Supplier (your business issuing the invoice).
    public string SupplierBusinessName { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierAddressLine1 { get; set; } = string.Empty;
    public string SupplierCity { get; set; } = string.Empty;
    public string SupplierCountry { get; set; } = string.Empty;

    // Optional supplier compliance identifiers.
    public string? SupplierGstNumber { get; set; }
    public string? SupplierNzbn { get; set; }

    // Client (bill-to party).
    public string ClientCompanyName { get; set; } = string.Empty;
    public string? ClientContactName { get; set; }
    public string? ClientEmail { get; set; }
    public string ClientAddressLine1 { get; set; } = string.Empty;
    public string ClientCity { get; set; } = string.Empty;
    public string ClientCountry { get; set; } = string.Empty;

    // Pricing snapshot (captured at time of invoice generation).
    public string RateTypeDisplay { get; set; } = string.Empty;
    public decimal RateValue { get; set; }
    public decimal Quantity { get; set; }

    // Calculated financial values.
    public decimal Subtotal { get; set; }
    public decimal GstAmount { get; set; }
    public decimal Total { get; set; }

    // GST configuration used during calculation.
    public bool GstEnabled { get; set; }
    public decimal GstRatePercent { get; set; }

    // Fuel surcharge configuration and calculated value.
    public bool FuelSurchargeEnabled { get; set; }
    public decimal FuelSurchargePercent { get; set; }
    public decimal FuelSurchargeAmount { get; set; }

    // Delivery timestamp (used for invoice dating and reporting).
    public DateTime DeliveredAtUtc { get; set; }
}