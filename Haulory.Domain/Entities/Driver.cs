using Domain.Entities;
using System.Text.Json.Serialization;

namespace Haulory.Domain.Entities
{
    public class Driver
    {
        [JsonInclude]
        public Guid Id { get; private set; } = Guid.NewGuid();

        // Owner/main account that owns this driver record
        [JsonInclude]
        public Guid OwnerUserId { get; private set; }

        // If this driver is the main user's own profile, link to the user account
        [JsonInclude]
        public Guid? UserId { get; private set; }

        [JsonInclude]
        public string? FirstName { get; private set; }

        [JsonInclude]
        public string? LastName { get; private set; }

        [JsonInclude]
        public string? Email { get; private set; }

        [JsonInclude]
        public string? LicenceNumber { get; private set; }

        [JsonInclude]
        public EmergencyContact EmergencyContact { get; private set; } = new EmergencyContact();

        [JsonInclude]
        public DriverStatus Status { get; private set; } = DriverStatus.Active;

        // Needed for JSON deserialization
        public Driver() { }

        public Driver(Guid ownerUserId, Guid? userId, string firstName, string lastName, string email)
        {
            OwnerUserId = ownerUserId;
            UserId = userId;

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
        }

        public void UpdateIdentity(string firstName, string lastName, string email)
        {
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
        }

        public void UpdateLicenceNumber(string? licenceNumber)
        {
            LicenceNumber = string.IsNullOrWhiteSpace(licenceNumber) ? null : licenceNumber.Trim();
        }

        public void UpdateEmergencyContact(EmergencyContact contact)
        {
            EmergencyContact = contact ?? new EmergencyContact();
        }

        public string DisplayName => $"{FirstName} {LastName}".Trim();

        // Helpful status string for UI
        public string EmergencyStatus
        {
            get
            {
                var ec = EmergencyContact;
                if (ec == null) return "Emergency contact not set";

                var missing = new List<string>();

                if (string.IsNullOrWhiteSpace(ec.FirstName)) missing.Add("first name");
                if (string.IsNullOrWhiteSpace(ec.LastName)) missing.Add("last name");
                if (string.IsNullOrWhiteSpace(ec.Relationship)) missing.Add("relationship");
                if (string.IsNullOrWhiteSpace(ec.PhoneNumber)) missing.Add("phone");
                if (string.IsNullOrWhiteSpace(ec.Email)) missing.Add("email");

                return missing.Count == 0
                    ? "Emergency contact set"
                    : $"Missing: {string.Join(", ", missing)}";
            }
        }

        public bool IsMainProfile => UserId.HasValue;

        public void EnsureOwner(Guid ownerUserId)
        {
            if (ownerUserId == Guid.Empty) return;

            if (OwnerUserId == Guid.Empty)
                OwnerUserId = ownerUserId;
        }

    }
}
