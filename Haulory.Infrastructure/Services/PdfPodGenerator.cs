using Haulory.Application.Features.Reports;
using Haulory.Application.Interfaces.Services;
using SkiaSharp;
using System.IO;
using System.Text.Json;

namespace Haulory.Infrastructure.Services;

public class PdfPodGenerator : IPdfPodGenerator
{
    public byte[] GeneratePodPdf(PodReportDto dto)
    {
        System.Diagnostics.Debug.WriteLine("========== PDF POD GENERATOR HIT ==========");
        using var stream = new MemoryStream();
        using var document = SKDocument.CreatePdf(stream);
        if (document == null)
            return Array.Empty<byte>();

        var width = 595;
        var height = 842;
        float margin = 40;
        float contentWidth = width - (margin * 2);

        float xLeft = margin;
        float xRight = width - margin;

        float yTop = 55;
        float line = 18;

        using var canvas = document.BeginPage(width, height);

        // ----- Fonts -----
        using var fontTitle = new SKFont { Size = 22, Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold) };
        using var fontBold = new SKFont { Size = 12, Typeface = SKTypeface.FromFamilyName(null, SKFontStyle.Bold) };
        using var fontNormal = new SKFont { Size = 12, Typeface = SKTypeface.Default };
        using var fontSmall = new SKFont { Size = 10, Typeface = SKTypeface.Default };

        // ----- Paint -----
        using var paintTitle = new SKPaint { IsAntialias = true, Color = SKColors.Black };
        using var paintBold = new SKPaint { IsAntialias = true, Color = SKColors.Black };
        using var paintNormal = new SKPaint { IsAntialias = true, Color = SKColors.Black };
        using var paintSmall = new SKPaint { IsAntialias = true, Color = SKColors.DarkSlateGray };

        // ===== HEADER =====

        // Title top-right
        canvas.DrawText("PROOF OF DELIVERY", xRight, yTop, SKTextAlign.Right, fontTitle, paintTitle);

        // Supplier block top-left
        float ySupplier = yTop;

        if (!string.IsNullOrWhiteSpace(dto.SupplierBusinessName))
        {
            canvas.DrawText(dto.SupplierBusinessName, xLeft, ySupplier, SKTextAlign.Left, fontBold, paintBold);
            ySupplier += line;
        }

        // Supplier address/email block
        var supplierLines = "";

        if (!string.IsNullOrWhiteSpace(dto.SupplierAddressLine1))
            supplierLines += dto.SupplierAddressLine1.Trim();

        if (!string.IsNullOrWhiteSpace(dto.SupplierCity))
            supplierLines += (supplierLines.Length > 0 ? "\n" : "") + dto.SupplierCity.Trim();

        if (!string.IsNullOrWhiteSpace(dto.SupplierCountry))
            supplierLines += (supplierLines.Length > 0 ? "\n" : "") + dto.SupplierCountry.Trim();

        if (!string.IsNullOrWhiteSpace(dto.SupplierEmail))
            supplierLines += (supplierLines.Length > 0 ? "\n" : "") + dto.SupplierEmail.Trim();

        if (!string.IsNullOrWhiteSpace(supplierLines))
        {
            ySupplier = PdfDrawHelpers.DrawMultiline(
                canvas,
                supplierLines,
                xLeft,
                ySupplier,
                fontNormal,
                paintNormal,
                SKTextAlign.Left,
                line);
        }

        if (!string.IsNullOrWhiteSpace(dto.SupplierGstNumber))
        {
            canvas.DrawText($"GST: {dto.SupplierGstNumber}", xLeft, ySupplier, SKTextAlign.Left, fontSmall, paintSmall);
            ySupplier += line;
        }

        if (!string.IsNullOrWhiteSpace(dto.SupplierNzbn))
        {
            canvas.DrawText($"NZBN: {dto.SupplierNzbn}", xLeft, ySupplier, SKTextAlign.Left, fontSmall, paintSmall);
            ySupplier += line;
        }

        // Move below header area
        float y = Math.Max(ySupplier, yTop + 36) + 12;

        PdfDrawHelpers.DrawHr(canvas, margin, y, width - margin);
        y += 18;

        // ===== POD DETAILS =====
        canvas.DrawText($"Reference #: {dto.ReferenceNumber}", xLeft, y, SKTextAlign.Left, fontNormal, paintNormal);
        y += line;

        if (!string.IsNullOrWhiteSpace(dto.InvoiceNumber))
        {
            canvas.DrawText($"Invoice #: {dto.InvoiceNumber}", xLeft, y, SKTextAlign.Left, fontNormal, paintNormal);
            y += line;
        }

        canvas.DrawText($"Delivered: {dto.DeliveredAtUtc.ToLocalTime():yyyy-MM-dd HH:mm}", xLeft, y, SKTextAlign.Left, fontNormal, paintNormal);
        y += 14;

        PdfDrawHelpers.DrawHr(canvas, margin, y, width - margin);
        y += 18;

        // ===== PICKUP =====
        canvas.DrawText("Pickup", xLeft, y, SKTextAlign.Left, fontBold, paintBold);
        y += line;

        y = PdfDrawHelpers.DrawMultiline(
            canvas,
            $"{dto.PickupCompany}\n{dto.PickupAddress}",
            xLeft,
            y,
            fontNormal,
            paintNormal,
            SKTextAlign.Left,
            line);

        y += 10;

        // ===== DELIVERY =====
        canvas.DrawText("Delivery", xLeft, y, SKTextAlign.Left, fontBold, paintBold);
        y += line;

        y = PdfDrawHelpers.DrawMultiline(
            canvas,
            $"{dto.DeliveryCompany}\n{dto.DeliveryAddress}",
            xLeft,
            y,
            fontNormal,
            paintNormal,
            SKTextAlign.Left,
            line);

        y += 12;

        // ===== LOAD =====
        if (!string.IsNullOrWhiteSpace(dto.LoadDescription))
        {
            canvas.DrawText("Load", xLeft, y, SKTextAlign.Left, fontBold, paintBold);
            y += line;

            y = PdfDrawHelpers.DrawMultiline(
                canvas,
                dto.LoadDescription,
                xLeft,
                y,
                fontNormal,
                paintNormal,
                SKTextAlign.Left,
                line);

            y += 6;
        }
        // ===== ADDITIONAL DETAILS =====
        if (!string.IsNullOrWhiteSpace(dto.DamageNotes) || dto.WaitTimeMinutes.HasValue)
        {
            System.Diagnostics.Debug.WriteLine(
    $"[PdfPodGenerator] DamageNotes='{dto.DamageNotes}', WaitTimeMinutes={dto.WaitTimeMinutes}");
            y += 8;
            PdfDrawHelpers.DrawHr(canvas, margin, y, width - margin);
            y += 18;

            canvas.DrawText("Additional Details", xLeft, y, SKTextAlign.Left, fontBold, paintBold);
            y += line;

            if (!string.IsNullOrWhiteSpace(dto.DamageNotes))
            {
                canvas.DrawText("Damage Notes", xLeft, y, SKTextAlign.Left, fontBold, paintBold);
                y += line;

                y = PdfDrawHelpers.DrawMultiline(
                    canvas,
                    dto.DamageNotes,
                    xLeft,
                    y,
                    fontNormal,
                    paintNormal,
                    SKTextAlign.Left,
                    line);

                y += 8;
            }

            if (dto.WaitTimeMinutes.HasValue)
            {
                canvas.DrawText(
                    $"Wait Time: {dto.WaitTimeMinutes.Value} minutes",
                    xLeft,
                    y,
                    SKTextAlign.Left,
                    fontNormal,
                    paintNormal);

                y += line;
            }
        }
        y += 10;
        PdfDrawHelpers.DrawHr(canvas, margin, y, width - margin);
        y += 18;

        // ===== RECEIVER =====
        canvas.DrawText("Receiver", xLeft, y, SKTextAlign.Left, fontBold, paintBold);
        y += line;

        canvas.DrawText($"Signed by: {dto.ReceiverName}", xLeft, y, SKTextAlign.Left, fontNormal, paintNormal);
        y += line;

        // ===== SIGNATURE BOX (full width) =====
        float boxX = margin;
        float boxW = contentWidth;
        float boxH = 160f;
        float boxY = y + 10;

        canvas.DrawText("Signature:", xLeft, y + 26, SKTextAlign.Left, fontSmall, paintSmall);

        using (var border = new SKPaint { Style = SKPaintStyle.Stroke, StrokeWidth = 1, Color = SKColors.LightGray })
            canvas.DrawRect(boxX, boxY, boxW, boxH, border);

        // Render from SignatureJson
        var signatureBytes = RenderSignaturePng(dto.SignatureJson);
        if (signatureBytes is { Length: > 0 })
        {
            using var bmp = SKBitmap.Decode(signatureBytes);
            if (bmp != null)
            {
                var scale = Math.Min(boxW / bmp.Width, boxH / bmp.Height);
                var drawW = bmp.Width * scale;
                var drawH = bmp.Height * scale;

                var imgX = boxX + (boxW - drawW) / 2f;
                var imgY = boxY + (boxH - drawH) / 2f;

                canvas.DrawBitmap(bmp, new SKRect(imgX, imgY, imgX + drawW, imgY + drawH));
            }
        }
        else
        {
            canvas.DrawText("(No signature captured)", boxX + 10, boxY + 30, SKTextAlign.Left, fontSmall, paintSmall);
        }

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

    private static byte[]? RenderSignaturePng(string signatureJson)
    {
        if (string.IsNullOrWhiteSpace(signatureJson))
            return null;

        SignatureData? data;
        try
        {
            data = JsonSerializer.Deserialize<SignatureData>(signatureJson);
        }
        catch
        {
            return null;
        }

        if (data?.Strokes == null || data.Strokes.Count == 0)
            return null;

        const int imgW = 900;
        const int imgH = 320;

        using var surface = SKSurface.Create(new SKImageInfo(imgW, imgH));
        if (surface == null)
            return null;

        var c = surface.Canvas;
        c.Clear(SKColors.Transparent);

        using var paint = new SKPaint
        {
            Color = SKColors.Black,
            StrokeWidth = 3,
            IsAntialias = true,
            Style = SKPaintStyle.Stroke,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        foreach (var stroke in data.Strokes)
        {
            if (stroke?.Points == null || stroke.Points.Count < 2)
                continue;

            for (int i = 1; i < stroke.Points.Count; i++)
            {
                var p1 = stroke.Points[i - 1];
                var p2 = stroke.Points[i];

                c.DrawLine(p1.X, p1.Y, p2.X, p2.Y, paint);
            }
        }

        using var image = surface.Snapshot();
        using var encoded = image.Encode(SKEncodedImageFormat.Png, 100);
        return encoded?.ToArray();
    }

    // Matches your SignatureJson models from DeliverySignatureViewModel
    private record SigPoint(float X, float Y);
    private record SignatureStroke(System.Collections.Generic.List<SigPoint> Points);
    private record SignatureData(System.Collections.Generic.List<SignatureStroke> Strokes);
}