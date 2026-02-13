using System.Text.Json.Serialization;

namespace Haulory.Domain.Entities
{
    public class User
    {
        [JsonInclude]
        public Guid Id { get; private set; } = Guid.NewGuid();

        [JsonInclude]
        public string? FirstName { get; private set; }

        [JsonInclude]
        public string? LastName { get; private set; }

        [JsonInclude]
        public string? Email { get; private set; }

        [JsonInclude]
        public string? PasswordHash { get; private set; }

        public User() { } // keep for JSON

        public User(string firstName, string lastName, string email, string passwordHash)
        {
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
            PasswordHash = passwordHash;
        }

        public void UpdateIdentity(string firstName, string lastName, string email)
        {
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
        }
    }
}
 
