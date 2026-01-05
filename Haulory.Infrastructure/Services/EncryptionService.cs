using System.Security.Cryptography;
using System.Text;

namespace Haulory.Infrastructure.Security;

public static class EncryptionService
{
    public static byte[] Encrypt(string plainText, byte[] key, out byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.GenerateIV();
        iv = aes.IV;

        using var encryptor = aes.CreateEncryptor();
        var bytes = Encoding.UTF8.GetBytes(plainText);

        return encryptor.TransformFinalBlock(bytes, 0, bytes.Length);
    }

    public static string Decrypt(byte[] cipherText, byte[] key, byte[] iv)
    {
        using var aes = Aes.Create();
        aes.Key = key;
        aes.IV = iv;

        using var decryptor = aes.CreateDecryptor();
        var decrypted = decryptor.TransformFinalBlock(cipherText, 0, cipherText.Length);

        return Encoding.UTF8.GetString(decrypted);
    }
}
