using HaulitCore.Application.Features.Reports;

namespace HaulitCore.Application.Interfaces.Services;

public interface IPdfInvoiceGenerator
{
    // Returns a ready-to-save PDF document as bytes.
    // signaturePngBytes can be empty if no signature captured.
    byte[] GenerateInvoicePdf(InvoiceReportDto dto, byte[] signaturePngBytes);
}