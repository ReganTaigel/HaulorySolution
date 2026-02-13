using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Infrastructure.Persistence.InMemory;

public class UserRepository : IUserRepository
{
    private readonly List<User> _users = new();

    public Task<bool> AnyAsync()
        => Task.FromResult(_users.Count > 0);

    public Task AddAsync(User user)
    {
        // prevent duplicate email
        if (_users.Any(u => u.Email == user.Email))
            return Task.CompletedTask;

        _users.Add(user);
        return Task.CompletedTask;
    }

    public Task<User?> GetByEmailAsync(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Task.FromResult<User?>(null);

        var normalized = email.Trim().ToLowerInvariant();
        var user = _users.FirstOrDefault(u => u.Email == normalized);

        return Task.FromResult(user);
    }

    // NEW
    public Task<User?> GetByIdAsync(Guid id)
    {
        if (id == Guid.Empty)
            return Task.FromResult<User?>(null);

        var user = _users.FirstOrDefault(u => u.Id == id);
        return Task.FromResult(user);
    }

    // NEW (this is what fixes the loop bug)
    public Task UpdateAsync(User user)
    {
        if (user.Id == Guid.Empty)
            throw new InvalidOperationException("Cannot update user with empty Id.");

        // email uniqueness
        if (_users.Any(u => u.Id != user.Id && u.Email == user.Email))
            throw new InvalidOperationException("That email is already in use.");

        var index = _users.FindIndex(u => u.Id == user.Id);
        if (index < 0)
        {
            // upsert
            _users.Add(user);
        }
        else
        {
            _users[index] = user;
        }

        return Task.CompletedTask;
    }
}
