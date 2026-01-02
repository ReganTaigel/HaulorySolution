using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Domain.Entities
{
    public class User
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string? Email { get; private set; }
        public string? PasswordHash { get; private set; }

        private User() { }

        public User(string firstName, string lastName, string email, string passwordHash)
        {
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
        }
    }
}
