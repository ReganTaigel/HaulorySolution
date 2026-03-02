using Haulory.Application.Features.Reports;

namespace Haulory.Application.Interfaces.Services;

public interface IPdfInvoiceGenerator
{
    // Returns a ready-to-save PDF document as bytes.
    // signaturePngBytes can be empty if no signature captured.
    byte[] GenerateInvoicePdf(InvoiceReportDto dto, byte[] signaturePngBytes);
}