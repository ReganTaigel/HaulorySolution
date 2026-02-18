using System.Security.Cryptography;
using System.Text;

namespace Haulory.Infrastructure.Security;

public static class EncryptionService
{
    #region Encryption

    public static byte[] Encrypt(string plainText, byte[] key, out byte[] iv)
    {
        // Create AES instance
        using var aes = Aes.Create();

        // Configure algorithm explicitly
        aes.Key = key;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        // Generate random IV for this encryption
        aes.GenerateIV();
        iv = aes.IV;

        using var encryptor = aes.CreateEncryptor();

        // Convert string to bytes
        var bytes = Encoding.UTF8.GetBytes(plainText);

        // Encrypt
        return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
    }

    #endregion

    #region Decryption

    public static string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        // Create AES instance
        using var aes = Aes.Create();

        // Configure algorithm explicitly
        aes.Key = key;
        aes.IV = iv;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var decryptor = aes.CreateDecryptor();

        // Decrypt
        var decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

        return Encoding.UTF8.GetString(decrypted);
    }

    #endregion
}
