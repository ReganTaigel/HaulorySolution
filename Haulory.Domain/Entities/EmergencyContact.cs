using System.Text.Json.Serialization;

namespace Haulory.Domain.Entities
{
    public class EmergencyContact
    {
        [JsonInclude] public string? FirstName { get; private set; }
        [JsonInclude] public string? LastName { get; private set; }
        [JsonInclude] public string? Relationship { get; private set; }
        [JsonInclude] public string? PhoneNumber { get; private set; }
        [JsonInclude] public string? SecondaryPhoneNumber { get; private set; }
        [JsonInclude] public string? Email { get; private set; }

        public EmergencyContact() { }

        public EmergencyContact(
            string firstname,
            string lastname,
            string relationship,
            string email,
            string phoneNumber,
            string? secondaryPhoneNumber = null)
        {
            FirstName = firstname.Trim();
            LastName = lastname.Trim();
            Relationship = relationship.Trim();
            Email = email.Trim();
            PhoneNumber = phoneNumber.Trim();
            SecondaryPhoneNumber = string.IsNullOrWhiteSpace(secondaryPhoneNumber)
                ? null
                : secondaryPhoneNumber.Trim();
        }

        public bool IsSet =>
            !string.IsNullOrWhiteSpace(FirstName) &&
            !string.IsNullOrWhiteSpace(LastName) &&
            !string.IsNullOrWhiteSpace(Relationship) &&
            !string.IsNullOrWhiteSpace(PhoneNumber) &&
            !string.IsNullOrWhiteSpace(Email);
    }
}
