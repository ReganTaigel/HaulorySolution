using Domain.Entities;

namespace Haulory.Domain.Entities
{
    public class Driver
    {
        public Guid Id { get; private set; } = Guid.NewGuid();

        // Owner/main account that owns this driver record
        public Guid OwnerUserId { get; private set; }

        // If this driver is the main user's own profile, link to the user account
        public Guid? UserId { get; private set; }

        public string? FirstName { get; private set; }
        public string? LastName { get; private set; }
        public string? Email { get; private set; }

        public string? PhoneNumber { get; private set; }

        public DateTime? DateOfBirthUtc { get; private set; }

        public string? LicenceNumber { get; private set; }
        public DateTime? LicenceExpiresOnUtc { get; private set; }

        public EmergencyContact EmergencyContact { get; private set; } = new EmergencyContact();

        // Address (stored directly on Driver as you requested)
        public string? Line1 { get; private set; }
        public string? Line2 { get; private set; }

        public string? Suburb { get; private set; }
        public string? City { get; private set; }
        public string? Region { get; private set; }

        public string? Postcode { get; private set; }
        public string? Country { get; private set; }

        public DriverStatus Status { get; private set; } = DriverStatus.Active;

        // Required by EF
        public Driver() { }

        public Driver(Guid ownerUserId, Guid? userId, string firstName, string lastName, string email)
        {
            OwnerUserId = ownerUserId;
            UserId = userId;

            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
        }

        // -------------------------
        // Identity
        // -------------------------

        public void UpdateIdentity(string firstName, string lastName, string email)
        {
            FirstName = firstName.Trim();
            LastName = lastName.Trim();
            Email = email.Trim().ToLowerInvariant();
        }

        public void UpdatePhone(string? phone)
        {
            PhoneNumber = Clean(phone);
        }

        public void UpdateDateOfBirthUtc(DateTime? dobUtc)
        {
            DateOfBirthUtc = dobUtc.HasValue
                ? DateTime.SpecifyKind(dobUtc.Value, DateTimeKind.Utc)
                : null;
        }

        // -------------------------
        // Licence
        // -------------------------

        public void UpdateLicenceNumber(string? licenceNumber)
        {
            LicenceNumber = Clean(licenceNumber);
        }

        public void UpdateLicenceExpiryUtc(DateTime? expiresUtc)
        {
            LicenceExpiresOnUtc = expiresUtc.HasValue
                ? DateTime.SpecifyKind(expiresUtc.Value, DateTimeKind.Utc)
                : null;
        }

        // -------------------------
        // Address
        // -------------------------

        public void UpdateAddress(
            string? line1,
            string? line2,
            string? suburb,
            string? city,
            string? region,
            string? postcode,
            string? country)
        {
            Line1 = Clean(line1);
            Line2 = Clean(line2);
            Suburb = Clean(suburb);
            City = Clean(city);
            Region = Clean(region);
            Postcode = Clean(postcode);
            Country = Clean(country);
        }

        // -------------------------
        // Emergency Contact
        // -------------------------

        public void UpdateEmergencyContact(EmergencyContact contact)
        {
            EmergencyContact = contact ?? new EmergencyContact();
        }

        // -------------------------
        // Helpers
        // -------------------------

        public string DisplayName => $"{FirstName} {LastName}".Trim();
        public string AddressSummary
        {
            get
            {
                var parts = new List<string>();

                if (!string.IsNullOrWhiteSpace(Line1)) parts.Add(Line1!.Trim());
                if (!string.IsNullOrWhiteSpace(Suburb)) parts.Add(Suburb!.Trim());
                if (!string.IsNullOrWhiteSpace(City)) parts.Add(City!.Trim());
                if (!string.IsNullOrWhiteSpace(Postcode)) parts.Add(Postcode!.Trim());
                if (!string.IsNullOrWhiteSpace(Country)) parts.Add(Country!.Trim());

                return string.Join(", ", parts);
            }
        }

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

        private static string? Clean(string? s) =>
            string.IsNullOrWhiteSpace(s) ? null : s.Trim();
    }
}
