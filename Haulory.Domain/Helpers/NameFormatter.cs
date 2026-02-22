using System.Globalization;

namespace Haulory.Domain.Helpers;

// Provides name formatting utilities for consistent identity normalization.
// Intended for use in Domain layer before persistence.
public static class NameFormatter
{
    // Converts a name to title case using current culture.
    // Handles trimming, casing, and multi-word names. 
    // Examples:
    // "regan" -> "Regan"
    // "MARY ANNE" -> "Mary Anne"
    // "john-smith" -> "John-Smith"
    public static string? ToTitleCase(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return value;

        var trimmed = value.Trim().ToLower();

        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(trimmed);
    }
}