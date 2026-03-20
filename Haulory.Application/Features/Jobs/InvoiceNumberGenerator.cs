using System.Text.RegularExpressions;

namespace Haulory.Application.Features.Jobs;

public static class InvoiceNumberGenerator
{
    private static readonly Regex DigitsRegex = new(@"(\d+)$", RegexOptions.Compiled);

    public static string GetNext(string? latestInvoiceNumber)
    {
        if (string.IsNullOrWhiteSpace(latestInvoiceNumber))
            return "INV-000001";

        var match = DigitsRegex.Match(latestInvoiceNumber.Trim());
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var current))
            return "INV-000001";

        return $"INV-{current + 1:D6}";
    }

    public static string Increment(string invoiceNumber)
    {
        var match = DigitsRegex.Match(invoiceNumber.Trim());
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var current))
            return "INV-000001";

        return $"INV-{current + 1:D6}";
    }
}