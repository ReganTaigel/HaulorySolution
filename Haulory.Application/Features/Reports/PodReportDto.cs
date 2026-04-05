namespace Haulory.Application.Features.Reports;

// Represents the data required to generate a Proof of Delivery (POD) report.
// Designed for document generation (e.g., PDF) showing delivery confirmation details.
public class PodReportDto
{
    // Identifiers for traceability.
    public Guid ReceiptId { get; set; }
    public Guid JobId { get; set; }

    // Supplier (your business issuing the POD).
    public string SupplierBusinessName { get; set; } = string.Empty;
    public string SupplierEmail { get; set; } = string.Empty;
    public string SupplierAddressLine1 { get; set; } = string.Empty;
    public string SupplierCity { get; set; } = string.Empty;
    public string SupplierCountry { get; set; } = string.Empty;

    // Optional supplier compliance identifiers.
    public string? SupplierGstNumber { get; set; }
    public string? SupplierNzbn { get; set; }

    // Reference and invoice identifiers.
    public string ReferenceNumber { get; set; } = string.Empty;
    public string InvoiceNumber { get; set; } = string.Empty;

    // Delivery timestamp.
    public DateTime DeliveredAtUtc { get; set; }

    // Pickup and delivery details.
    public string PickupCompany { get; set; } = string.Empty;
    public string PickupAddress { get; set; } = string.Empty;
    public string DeliveryCompany { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;

    // Description of goods transported.
    public string LoadDescription { get; set; } = string.Empty;

    // Receiver confirmation details.
    public string ReceiverName { get; set; } = string.Empty;

    // Serialized signature data (used for rendering signature in POD).
    public string SignatureJson { get; set; } = string.Empty;

    // Optional delivery notes.
    public string? DamageNotes { get; set; }
    public int? WaitTimeMinutes { get; set; }
}