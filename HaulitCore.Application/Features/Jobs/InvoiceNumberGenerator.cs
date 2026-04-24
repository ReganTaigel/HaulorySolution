using System.Text.RegularExpressions;

namespace HaulitCore.Application.Features.Jobs;

public static class InvoiceNumberGenerator
{
    private static readonly Regex DigitsRegex = new(@"(\d+)$", RegexOptions.Compiled);

    public static string GetNext(string? latestInvoiceNumber, string prefix)
    {
        prefix = string.IsNullOrWhiteSpace(prefix) ? "INV" : prefix.Trim().ToUpperInvariant();

        if (string.IsNullOrWhiteSpace(latestInvoiceNumber))
            return $"{prefix}-000001";

        var match = DigitsRegex.Match(latestInvoiceNumber.Trim());

        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var current))
            return $"{prefix}-000001";

        return $"{prefix}-{current + 1:D6}";
    }

    public static string Increment(string invoiceNumber, string prefix)
    {
        prefix = string.IsNullOrWhiteSpace(prefix) ? "INV" : prefix.Trim().ToUpperInvariant();

        var match = DigitsRegex.Match(invoiceNumber.Trim());

        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var current))
            return $"{prefix}-000001";

        return $"{prefix}-{current + 1:D6}";
    }
}