using System.Text.Json;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Security;

namespace Haulory.Infrastructure.Persistence.Json;

public class UserRepository : IUserRepository
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    public UserRepository()
    {
        _filePath = Path.Combine(
            FileSystem.AppDataDirectory,
            "users.dat");
    }

    public async Task AddAsync(User user)
    {
        await _lock.WaitAsync();

        try
        {
            var users = await LoadAsync();

            if (users.Any(u => u.Email == user.Email))
                return;

            users.Add(user);
            await SaveAsync(users);
        }
        finally
        {
            _lock.Release();
        }
    } 

    public async Task<User?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLowerInvariant();
        var users = await LoadAsync();

        return users.FirstOrDefault(u => u.Email == normalized);
    }

    // ------------------------
    // Encrypted JSON Helpers
    // ------------------------

    private async Task<List<User>> LoadAsync()
    {
        if (!File.Exists(_filePath))
            return new List<User>();

        var key = await KeyService.GetOrCreateKeyAsync();
        var bytes = await File.ReadAllBytesAsync(_filePath);

        var iv = bytes[..16];
        var cipher = bytes[16..];

        var json = EncryptionService.Decrypt(cipher, key, iv);

        return JsonSerializer.Deserialize<List<User>>(json)
               ?? new List<User>();
    }

    private async Task SaveAsync(List<User> users)
    {
        var key = await KeyService.GetOrCreateKeyAsync();

        var json = JsonSerializer.Serialize(users,
            new JsonSerializerOptions { WriteIndented = true });

        var cipher = EncryptionService.Encrypt(json, key, out var iv);

        var combined = iv.Concat(cipher).ToArray();
        await File.WriteAllBytesAsync(_filePath, combined);
    }
}
