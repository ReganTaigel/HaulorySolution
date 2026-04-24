using System.Linq;

namespace HaulitCore.Application.Security;

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

        // Require at least 1 digit
        int digitCount = password.Count(char.IsDigit);
        if (digitCount < 1)
        {
            errorMessage = "Password must contain at least 1 number.";
            return false;
        }

        // Require at least 1 special character
        int specialCount = password.Count(c => !char.IsLetterOrDigit(c));
        if (specialCount < 1)
        {
            errorMessage = "Password must contain at least 1 special character.";
            return false;
        }

        // Require at least 1 uppercase letter
        int upperCount = password.Count(char.IsUpper);
        if (upperCount < 1)
        {
            errorMessage = "Password must contain at least 1 uppercase letter.";
            return false;
        }

        return true;
    }

    #endregion
}

#endregion
