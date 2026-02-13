using System.Text.Json;
using Haulory.Infrastructure.Security;
using Haulory.Infrastructure.Services;

namespace Haulory.Infrastructure.Storage;

public static class EncryptedJsonStore
{
    private const int IvSize = 16;

    private static JsonSerializerOptions DefaultOptions => new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public static async Task SaveAsync<T>(
        string filePath,
        T data,
        JsonSerializerOptions? options = null)
    {
        var key = await KeyService.GetOrCreateKeyAsync();

        var json = JsonSerializer.Serialize(data, options ?? DefaultOptions);
        var cipher = EncryptionService.Encrypt(json, key, out var iv);

        // store [IV][CIPHERTEXT]
        var combined = new byte[iv.Length + cipher.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(cipher, 0, combined, iv.Length, cipher.Length);

        await WriteAtomicAsync(filePath, combined);
    }

    public static async Task<T?> LoadAsync<T>(
    string filePath,
    JsonSerializerOptions? options = null)
    {
        if (!File.Exists(filePath))
            return default;

        try
        {
            var key = await KeyService.GetOrCreateKeyAsync();

            var combined = await File.ReadAllBytesAsync(filePath);
            if (combined.Length <= IvSize)
                throw new InvalidOperationException("Encrypted file is invalid (missing IV or data).");

            var iv = new byte[IvSize];
            Buffer.BlockCopy(combined, 0, iv, 0, IvSize);

            var cipher = new byte[combined.Length - IvSize];
            Buffer.BlockCopy(combined, IvSize, cipher, 0, cipher.Length);

            var json = EncryptionService.Decrypt(cipher, key, iv);
            if (string.IsNullOrWhiteSpace(json))
                return default;

            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }
        catch
        {
            // ✅ quarantine unreadable encrypted file
            try
            {
                var badPath = filePath + ".bad.decrypt";
                if (File.Exists(badPath)) File.Delete(badPath);
                File.Move(filePath, badPath);
            }
            catch
            {
                // ignore quarantine failure
            }

            return default;
        }
    }


    private static async Task WriteAtomicAsync(string filePath, byte[] bytes)
    {
        var dir = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(dir);

        var tmp = filePath + ".tmp";
        var bak = filePath + ".bak";

        await File.WriteAllBytesAsync(tmp, bytes);

        if (File.Exists(filePath))
        {
            try
            {
                File.Replace(tmp, filePath, bak, ignoreMetadataErrors: true);
                return;
            }
            catch
            {
                // fall through
            }
        }

        if (File.Exists(filePath))
            File.Delete(filePath);

        File.Move(tmp, filePath);
    }
}
