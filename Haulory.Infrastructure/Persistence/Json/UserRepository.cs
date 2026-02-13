using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Storage;
using Microsoft.Maui.Storage;
using System.Text.Json;

namespace Haulory.Infrastructure.Persistence.Json;

public class UserRepository : IUserRepository
{
    private readonly string _filePath;
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public UserRepository()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "users.json.enc");
    }

    public async Task<bool> AnyAsync()
    {
        await _lock.WaitAsync();
        try
        {
            var users = await LoadAsync();
            return users.Count > 0;
        }
        finally { _lock.Release(); }
    }

    public async Task AddAsync(User user)
    {
        await _lock.WaitAsync();
        try
        {
            var users = await LoadAsync();

            // ✅ Prevent duplicates by email
            if (users.Any(u => u.Email == user.Email))
                return;

            users.Add(user);
            await SaveAsync(users);
        }
        finally { _lock.Release(); }
    }

    public async Task<User?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return null;

        var normalized = email.Trim().ToLowerInvariant();

        await _lock.WaitAsync();
        try
        {
            var users = await LoadAsync();
            return users.FirstOrDefault(u => u.Email == normalized);
        }
        finally { _lock.Release(); }
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty) return null;

        await _lock.WaitAsync();
        try
        {
            var users = await LoadAsync();
            return users.FirstOrDefault(u => u.Id == id);
        }
        finally { _lock.Release(); }
    }

    public async Task UpdateAsync(User user)
    {
        if (user.Id == Guid.Empty)
            throw new InvalidOperationException("Cannot update user with empty Id.");

        await _lock.WaitAsync();
        try
        {
            var users = await LoadAsync();

            // ✅ Don't allow updating to an email that belongs to someone else
            if (users.Any(u => u.Id != user.Id && u.Email == user.Email))
                throw new InvalidOperationException("That email is already in use.");

            var idx = users.FindIndex(u => u.Id == user.Id);
            if (idx < 0)
            {
                // If missing, add it (upsert)
                users.Add(user);
            }
            else
            {
                users[idx] = user;
            }

            await SaveAsync(users);
        }
        finally { _lock.Release(); }
    }

    private async Task<List<User>> LoadAsync()
    {
        var data = await EncryptedJsonStore.LoadAsync<List<User>>(_filePath, JsonOptions);
        return data ?? new List<User>();
    }

    private async Task SaveAsync(List<User> users)
    {
        await EncryptedJsonStore.SaveAsync(_filePath, users, JsonOptions);
    }
}
