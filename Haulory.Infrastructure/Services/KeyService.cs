using System.Security.Cryptography;

namespace Haulory.Infrastructure.Services;

public static class KeyService
{
    private const string KeyName = "haulory_user_store_key";

    public static async Task<byte[]> GetOrCreateKeyAsync()
    {
        try
        {
            var stored = await SecureStorage.GetAsync(KeyName);

            if (!string.IsNullOrWhiteSpace(stored))
                return Convert.FromBase64String(stored);
        }
        catch (Exception ex)
        {
            // Never generate a new key silently
            throw new InvalidOperationException(
                "SecureStorage unavailable. Cannot access encryption key.",
                ex);
        }

        // FIRST INSTALL ONLY
        var key = RandomNumberGenerator.GetBytes(32); // 256-bit key

        await SecureStorage.SetAsync(
            KeyName,
            Convert.ToBase64String(key));

        return key;
    }
}
