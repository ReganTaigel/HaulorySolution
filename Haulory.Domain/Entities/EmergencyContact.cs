using Haulory.Domain.Helpers;

namespace Haulory.Domain.Entities;

#region Value Object: Emergency Contact

public class EmergencyContact
{
    #region Properties

    public string? FirstName { get; private set; }
    public string? LastName { get; private set; }
    public string? Relationship { get; private set; }

    public string? PhoneNumber { get; private set; }
    public string? SecondaryPhoneNumber { get; private set; }

    public string? Email { get; private set; }

    #endregion

    #region Constructors

    // Required for EF Core (owned entity materialization)
    public EmergencyContact() { }

    public EmergencyContact(
        string firstname,
        string lastname,
        string relationship,
        string email,
        string phoneNumber,
        string? secondaryPhoneNumber = null)
    {
        // Normalize identity fields for consistent display and auditing
        FirstName = NameFormatter.ToTitleCase(firstname);
        LastName = NameFormatter.ToTitleCase(lastname);

        Relationship = Clean(relationship);

        Email = CleanEmail(email);
        PhoneNumber = Clean(phoneNumber);

        SecondaryPhoneNumber = Clean(secondaryPhoneNumber);
    }

    #endregion

    #region Derived State

    // True when all required emergency fields are populated
    public bool IsSet =>
        !string.IsNullOrWhiteSpace(FirstName) &&
        !string.IsNullOrWhiteSpace(LastName) &&
        !string.IsNullOrWhiteSpace(Relationship) &&
        !string.IsNullOrWhiteSpace(PhoneNumber) &&
        !string.IsNullOrWhiteSpace(Email);

    #endregion

    #region Helpers

    private static string? Clean(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string? CleanEmail(string? email)
    {
        var cleaned = Clean(email);
        return cleaned?.ToLowerInvariant();
    }

    #endregion
}

#endregion