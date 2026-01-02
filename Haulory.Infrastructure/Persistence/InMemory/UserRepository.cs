using Haulory.Application.Interfaces.Repositories;
using Haulory.Domain.Entities;

namespace Haulory.Infrastructure.Persistence.InMemory
{
    public class UserRepository : IUserRepository
    {
        private static readonly List<User> _users = new();

        public Task AddAsync(User user)
        {
            // Safety net: prevent duplicate emails
            if (_users.Any(u => u.Email == user.Email))
                return Task.CompletedTask;

            _users.Add(user);
            return Task.CompletedTask;
        }
        public Task<User?> GetByEmailAsync(string? email)
        {
            if (string.IsNullOrWhiteSpace(email))
                return Task.FromResult<User?>(null);

            var normalizedEmail = email.Trim().ToLowerInvariant();

            var user = _users.FirstOrDefault(u =>
                u.Email == normalizedEmail);

            return Task.FromResult(user);
        }
    }
}
