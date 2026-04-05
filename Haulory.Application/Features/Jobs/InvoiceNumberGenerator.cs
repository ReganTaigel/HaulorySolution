using System.Text.RegularExpressions;

namespace Haulory.Application.Features.Jobs;

// Provides helper methods for generating and incrementing invoice numbers.
// Ensures a consistent format: INV-000001, INV-000002, etc.
public static class InvoiceNumberGenerator
{
    // Regex used to extract the numeric suffix at the end of an invoice number.
    private static readonly Regex DigitsRegex = new(@"(\d+)$", RegexOptions.Compiled);

    // Generates the next invoice number based on the latest known invoice number.
    public static string GetNext(string? latestInvoiceNumber)
    {
        // If no previous invoice exists, start from the default.
        if (string.IsNullOrWhiteSpace(latestInvoiceNumber))
            return "INV-000001";

        // Extract trailing digits from the latest invoice number.
        var match = DigitsRegex.Match(latestInvoiceNumber.Trim());

        // If extraction fails or parsing fails, reset to default.
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var current))
            return "INV-000001";

        // Increment and format with leading zeros (6 digits).
        return $"INV-{current + 1:D6}";
    }

    // Increments an existing invoice number.
    public static string Increment(string invoiceNumber)
    {
        // Extract trailing digits from the invoice number.
        var match = DigitsRegex.Match(invoiceNumber.Trim());

        // If extraction fails or parsing fails, reset to default.
        if (!match.Success || !int.TryParse(match.Groups[1].Value, out var current))
            return "INV-000001";

        // Increment and format with leading zeros (6 digits).
        return $"INV-{current + 1:D6}";
    }
}