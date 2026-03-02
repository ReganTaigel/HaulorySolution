using Haulory.Application.Features.Reports;
using Haulory.Application.Interfaces.Services;
using SkiaSharp;
using System.Globalization;
using System.IO;

namespace Haulory.Infrastructure.Services;

public class PdfInvoiceGenerator : IPdfInvoiceGenerator
{
    public byte[] GenerateInvoicePdf(InvoiceReportDto dto, byte[] signaturePngBytes)
    {
        var culture = CultureInfo.GetCultureInfo("en-NZ");

        using var stream = new MemoryStream();
        using var document = SKDocument.CreatePdf(stream);
        if (document == null)
            return Array.Empty<byte>();

        var width = 595;
        var height = 842;
        float margin = 40;
        float contentWidth = width - (margin * 2);

        using var canvas = document.BeginPage(width, height);

        // ----- Fonts -----
        using var fontTitle = new SKFont { Size = 22, Typeface = SKTypeface.Default };
        using var fontBold = new SKFont { Size = 12, Typeface = SKTypeface.Default };
        using var fontNormal = new SKFont { Size = 12, Typeface = SKTypeface.Default };
        using var fontSmall = new SKFont { Size = 10, Typeface = SKTypeface.Default };

        // ----- Paint (Style Only) -----
        using var paintTitle = new SKPaint { IsAntialias = true, Color = SKColors.Black };
        using var paintBold = new SKPaint { IsAntialias = true, Color = SKColors.Black };
        using var paintNormal = new SKPaint { IsAntialias = true, Color = SKColors.Black };
        using var paintSmall = new SKPaint { IsAntialias = true, Color = SKColors.DarkSlateGray };

        float xLeft = margin;
        float xRight = width - margin;
        float yTop = 55;
        float line = 18;

        // ===== TITLE =====
        canvas.DrawText("INVOICE", xRight, yTop, SKTextAlign.Right, fontTitle, paintTitle);

        // ===== TOP BLOCKS =====
        float yClient = yTop + 36;
        float ySupplier = yTop + 36;

        // ---- Client (Top Left) ----
        canvas.DrawText("Bill To (Client)", xLeft, yClient, SKTextAlign.Left, fontBold, paintBold);
        yClient += line;

        var clientLines =
            $"{dto.ClientCompanyName}\n" +
            $"{dto.ClientAddressLine1}\n" +
            $"{dto.ClientCity}\n" +
            $"{dto.ClientCountry}";

        if (!string.IsNullOrWhiteSpace(dto.ClientContactName))
            clientLines = $"{dto.ClientContactName}\n" + clientLines;

        if (!string.IsNullOrWhiteSpace(dto.ClientEmail))
            clientLines += $"\n{dto.ClientEmail}";

        yClient = PdfDrawHelpers.DrawMultiline(
            canvas,
            clientLines,
            xLeft,
            yClient,
            fontNormal,
            paintNormal,
            SKTextAlign.Left,
            line);

        // ---- Supplier (Top Right) ----
        // These should already be BUSINESS snapshot fields from the receipt.
        canvas.DrawText(dto.SupplierBusinessName ?? string.Empty, xRight, ySupplier, SKTextAlign.Right, fontBold, paintBold);
        ySupplier += line;

        var supplierLines =
            $"{dto.SupplierAddressLine1}\n" +
            $"{dto.SupplierCity}\n" +
            $"{dto.SupplierCountry}\n" +
            $"{dto.SupplierEmail}";

        ySupplier = PdfDrawHelpers.DrawMultiline(
            canvas,
            supplierLines,
            xRight,
            ySupplier,
            fontNormal,
            paintNormal,
            SKTextAlign.Right,
            line);

        if (!string.IsNullOrWhiteSpace(dto.SupplierGstNumber))
        {
            canvas.DrawText($"GST: {dto.SupplierGstNumber}", xRight, ySupplier, SKTextAlign.Right, fontSmall, paintSmall);
            ySupplier += line;
        }

        if (!string.IsNullOrWhiteSpace(dto.SupplierNzbn))
        {
            canvas.DrawText($"NZBN: {dto.SupplierNzbn}", xRight, ySupplier, SKTextAlign.Right, fontSmall, paintSmall);
            ySupplier += line;
        }

        // Move below whichever block ends lowest
        float y = Math.Max(yClient, ySupplier) + 12;

        // Divider
        PdfDrawHelpers.DrawHr(canvas, margin, y, width - margin);
        y += 18;

        // ===== INVOICE DETAILS (just above table) =====
        canvas.DrawText($"Invoice #: {dto.InvoiceNumber}", xLeft, y, SKTextAlign.Left, fontNormal, paintNormal);
        y += line;

        canvas.DrawText($"Reference #: {dto.ReferenceNumber}", xLeft, y, SKTextAlign.Left, fontNormal, paintNormal);
        y += line;

        canvas.DrawText($"Invoice date: {dto.DeliveredAtUtc.ToLocalTime():yyyy-MM-dd}", xLeft, y, SKTextAlign.Left, fontNormal, paintNormal);
        y += 14;

        PdfDrawHelpers.DrawHr(canvas, margin, y, width - margin);
        y += 18;

        // ===== TABLE =====
        float col1 = margin;
        float col2 = margin + contentWidth * 0.52f;
        float col3 = margin + contentWidth * 0.72f;
        float col4 = margin + contentWidth * 0.98f;

        // Headers
        canvas.DrawText("Rate Type", col1, y, SKTextAlign.Left, fontBold, paintBold);
        canvas.DrawText("Rate", col2, y, SKTextAlign.Left, fontBold, paintBold);
        canvas.DrawText("Qty", col3, y, SKTextAlign.Left, fontBold, paintBold);
        canvas.DrawText("Line Total", col4, y, SKTextAlign.Right, fontBold, paintBold);

        y += 14;
        PdfDrawHelpers.DrawHr(canvas, margin, y, width - margin);
        y += 18;

        // Line item
        canvas.DrawText(dto.RateTypeDisplay, col1, y, SKTextAlign.Left, fontNormal, paintNormal);
        canvas.DrawText(dto.RateValue.ToString("C", culture), col2, y, SKTextAlign.Left, fontNormal, paintNormal);
        canvas.DrawText(dto.Quantity.ToString("0.##", culture), col3, y, SKTextAlign.Left, fontNormal, paintNormal);

        // Line total is the subtotal (single line item)
        canvas.DrawText(dto.Subtotal.ToString("C", culture), col4, y, SKTextAlign.Right, fontNormal, paintNormal);
        y += 26;

        void DrawRight(string label, string value, bool bold = false)
        {
            var font = bold ? fontBold : fontNormal;
            var paint = bold ? paintBold : paintNormal;
            var text = $"{label} {value}";

            canvas.DrawText(text, col4, y, SKTextAlign.Right, font, paint);
        }

        DrawRight("Subtotal:", dto.Subtotal.ToString("C", culture));
        y += line;

        if (dto.GstAmount > 0)
        {
            DrawRight("GST:", dto.GstAmount.ToString("C", culture));
            y += line;
        }

        DrawRight("Total:", dto.Total.ToString("C", culture), bold: true);

        // Footer
        canvas.DrawText(
            $"Generated by Haulory • {DateTime.Now:yyyy-MM-dd HH:mm}",
            margin,
            height - 30,
            SKTextAlign.Left,
            fontSmall,
            paintSmall);

        document.EndPage();
        document.Close();

        return stream.ToArray();
    }
}