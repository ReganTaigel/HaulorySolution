using HaulitCore.Contracts.Drivers;
using HaulitCore.Domain.Entities;

namespace HaulitCore.Api.Extensions;

// Provides mapping extensions for converting Driver domain entities into DTOs.
public static class DriverMappingExtensions
{
    // Maps a Driver entity to a DriverDto for API responses.
    public static DriverDto ToDto(this Driver driver)
    {
        return new DriverDto
        {
            Id = driver.Id,
            OwnerUserId = driver.OwnerUserId,
            UserId = driver.UserId,

            // Identity and contact details.
            FirstName = driver.FirstName,
            LastName = driver.LastName,
            DisplayName = driver.DisplayName,
            Email = driver.Email,
            PhoneNumber = driver.PhoneNumber,
            DateOfBirthUtc = driver.DateOfBirthUtc,

            // Licence and compliance information.
            LicenceNumber = driver.LicenceNumber,
            LicenceVersion = driver.LicenceVersion,
            LicenceClassOrEndorsements = driver.LicenceClassOrEndorsements,
            LicenceIssuedOnUtc = driver.LicenceIssuedOnUtc,
            LicenceExpiresOnUtc = driver.LicenceExpiresOnUtc,
            LicenceConditionsNotes = driver.LicenceConditionsNotes,

            // Address details.
            Line1 = driver.Line1,
            Suburb = driver.Suburb,
            City = driver.City,
            Region = driver.Region,
            Postcode = driver.Postcode,
            Country = driver.Country,

            // Pre-formatted address summary (likely computed in domain).
            AddressSummary = driver.AddressSummary,

            // Convert status enum to string for API consumption.
            Status = driver.Status.ToString(),

            // Emergency contact details (nullable-safe mapping).
            EmergencyContact = new EmergencyContactDto
            {
                FirstName = driver.EmergencyContact?.FirstName,
                LastName = driver.EmergencyContact?.LastName,
                Relationship = driver.EmergencyContact?.Relationship,
                PhoneNumber = driver.EmergencyContact?.PhoneNumber,
                SecondaryPhoneNumber = driver.EmergencyContact?.SecondaryPhoneNumber,
                Email = driver.EmergencyContact?.Email
            },

            // Additional derived/aggregate state.
            EmergencyStatus = driver.EmergencyStatus,
            IsMainProfile = driver.IsMainProfile
        };
    }
}