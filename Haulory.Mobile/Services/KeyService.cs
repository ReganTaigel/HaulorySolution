using System;
using System.Security.Cryptography;
using Microsoft.Maui.Storage;

namespace Haulory.Mobile.Services;

public static class KeyService
{
    #region Constants

    private const string KeyName = "haulory_user_store_key";

    #endregion

    #region Public API

    public static async Task<byte[]> GetOrCreateKeyAsync()
    {
        try
        {
            // Attempt to retrieve existing key from secure storage
            var stored = await SecureStorage.GetAsync(KeyName);

            if (!string.IsNullOrWhiteSpace(stored))
                return Convert.FromBase64String(stored);
        }
        catch (Exception ex)
        {
            // Never silently generate a new key if secure storage fails
            // This prevents accidental data loss from key mismatch
            throw new InvalidOperationException(
                "SecureStorage unavailable. Cannot access encryption key.",
                ex);
        }

        // First install only
        // Generate new 256-bit encryption key
        var key = RandomNumberGenerator.GetBytes(32);

        await SecureStorage.SetAsync(
            KeyName,
            Convert.ToBase64String(key));

        return key;
    }

    #endregion
}
