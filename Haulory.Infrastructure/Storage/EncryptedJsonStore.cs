using System;
using System.IO;
using System.Text.Json;
using Haulory.Infrastructure.Security;
using Haulory.Infrastructure.Services;

namespace Haulory.Infrastructure.Storage;

public static class EncryptedJsonStore
{
    #region Constants

    // AES IV size in bytes (128-bit)
    private const int IvSize = 16;

    #endregion

    #region Json Options

    private static JsonSerializerOptions DefaultOptions => new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    #endregion

    #region Public API

    public static async Task SaveAsync<T>(
        string filePath,
        T data,
        JsonSerializerOptions? options = null)
    {
        // Retrieve or create device key
        var key = await KeyService.GetOrCreateKeyAsync();

        // Serialize to JSON
        var json = JsonSerializer.Serialize(data, options ?? DefaultOptions);

        // Encrypt JSON -> ciphertext + IV
        var cipher = EncryptionService.Encrypt(json, key, out var iv);

        // Store as [IV][CIPHERTEXT]
        var combined = new byte[iv.Length + cipher.Length];
        Buffer.BlockCopy(iv, 0, combined, 0, iv.Length);
        Buffer.BlockCopy(cipher, 0, combined, iv.Length, cipher.Length);

        // Write atomically to avoid partial/corrupt files
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
            // Retrieve key from secure storage
            var key = await KeyService.GetOrCreateKeyAsync();

            // Read encrypted bytes
            var combined = await File.ReadAllBytesAsync(filePath);

            // Must include IV + ciphertext
            if (combined.Length <= IvSize)
                throw new InvalidOperationException("Encrypted file is invalid (missing IV or data).");

            // Extract IV
            var iv = new byte[IvSize];
            Buffer.BlockCopy(combined, 0, iv, 0, IvSize);

            // Extract ciphertext
            var cipher = new byte[combined.Length - IvSize];
            Buffer.BlockCopy(combined, IvSize, cipher, 0, cipher.Length);

            // Decrypt -> JSON
            var json = EncryptionService.Decrypt(cipher, key, iv);
            if (string.IsNullOrWhiteSpace(json))
                return default;

            // Deserialize JSON -> object
            return JsonSerializer.Deserialize<T>(json, options ?? DefaultOptions);
        }
        catch
        {
            // Quarantine unreadable encrypted file to prevent repeated failures
            try
            {
                var badPath = filePath + ".bad.decrypt";

                if (File.Exists(badPath))
                    File.Delete(badPath);

                File.Move(filePath, badPath);
            }
            catch
            {
                // Ignore quarantine failure
            }

            return default;
        }
    }

    #endregion

    #region Atomic Write

    private static async Task WriteAtomicAsync(string filePath, byte[] bytes)
    {
        // Ensure directory exists
        var dir = Path.GetDirectoryName(filePath)!;
        Directory.CreateDirectory(dir);

        var tmp = filePath + ".tmp";
        var bak = filePath + ".bak";

        // Write to a temp file first
        await File.WriteAllBytesAsync(tmp, bytes);

        // Prefer OS-level replace if target exists
        if (File.Exists(filePath))
        {
            try
            {
                File.Replace(tmp, filePath, bak, ignoreMetadataErrors: true);
                return;
            }
            catch
            {
                // Fall through to manual move
            }
        }

        // Manual replace fallback
        if (File.Exists(filePath))
            File.Delete(filePath);

        File.Move(tmp, filePath);
    }

    #endregion
}
