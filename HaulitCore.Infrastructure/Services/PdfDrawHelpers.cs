using SkiaSharp;

namespace HaulitCore.Infrastructure.Services;

internal static class PdfDrawHelpers
{
    public static void DrawHr(SKCanvas canvas, float x1, float y, float x2)
    {
        using var hr = new SKPaint { Color = SKColors.LightGray, StrokeWidth = 1 };
        canvas.DrawLine(x1, y, x2, y, hr);
    }

    public static float DrawMultiline(
        SKCanvas canvas,
        string text,
        float x,
        float y,
        SKFont font,
        SKPaint paint,
        SKTextAlign textAlign,
        float lineHeight)
    {
        var lines = (text ?? string.Empty).Split('\n');

        foreach (var line in lines)
        {
            canvas.DrawText(
                line,
                x,
                y,
                textAlign,
                font,
                paint);

            y += lineHeight;
        }

        return y;
    }
}