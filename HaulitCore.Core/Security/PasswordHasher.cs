using System.Security.Cryptography;
using System.Text;

namespace HaulitCore.Core.Security;

#region Class: Password Hasher

public class PasswordHasher
{
    #region Hashing

    // Creates a SHA256 hash of the provided password
    // Returns Base64-encoded string representation
    public static string Hash(string password)
    {
        using var sha256 = SHA256.Create();

        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = sha256.ComputeHash(bytes);

        return Convert.ToBase64String(hash);
    }

    #endregion

    #region Verification

    // Verifies a password against a stored hash
    public static bool Verify(string password, string hash)
        => Hash(password) == hash;

    #endregion
}

#endregion
