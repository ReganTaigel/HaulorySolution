using System.Text.RegularExpressions;

namespace Haulory.Application.Security;

public static class PasswordPolicy
{
    public static bool IsValid(
        string password,
        out string errorMessage)
    {
        errorMessage = string.Empty;

        if (string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "Password is required.";
            return false;
        }

        if (password.Length < 8)
        {
            errorMessage = "Password must be at least 8 characters long.";
            return false;
        }

        int digitCount = password.Count(char.IsDigit);
        if (digitCount < 2)
        {
            errorMessage = "Password must contain at least 2 numbers.";
            return false;
        }

        int specialCount = password.Count(c => !char.IsLetterOrDigit(c));
        if (specialCount < 2)
        {
            errorMessage = "Password must contain at least 2 special characters.";
            return false;
        }

        return true;
    }
}
