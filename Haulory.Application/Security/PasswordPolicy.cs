using System.Linq;

namespace Haulory.Application.Security;

#region Static Class: Password Policy

public static class PasswordPolicy
{
    #region Validation

    public static bool IsValid(
        string password,
        out string errorMessage)
    {
        errorMessage = string.Empty;

        // Must not be empty
        if (string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "Password is required.";
            return false;
        }

        // Minimum length requirement
        if (password.Length < 8)
        {
            errorMessage = "Password must be at least 8 characters long.";
            return false;
        }

        // Require at least 2 digits
        int digitCount = password.Count(char.IsDigit);
        if (digitCount < 1)
        {
            errorMessage = "Password must contain at least 1 numbers.";
            return false;
        }

        // Require at least 2 special characters
        int specialCount = password.Count(c => !char.IsLetterOrDigit(c));
        if (specialCount < 1)
        {
            errorMessage = "Password must contain at least 1 special characters.";
            return false;
        }

        return true;
    }

    #endregion
}

#endregion
