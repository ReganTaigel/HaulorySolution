using System.Security.Cryptography;

namespace Haulory.Infrastructure.Security;

public static class KeyService
{
    private const string KeyName = "haulory_user_store_key";

    public static async Task<byte[]> GetOrCreateKeyAsync()
    {
        var stored = await SecureStorage.GetAsync(KeyName);

        if (stored != null)
            return Convert.FromBase64String(stored);

        var key = RandomNumberGenerator.GetBytes(32); // 256-bit key
        await SecureStorage.SetAsync(KeyName, Convert.ToBase64String(key));

        return key;
    }
}
