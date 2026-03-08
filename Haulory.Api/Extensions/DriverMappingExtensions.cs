using Haulory.Api.Contracts.Drivers;
using Haulory.Domain.Entities;

namespace Haulory.Api.Extensions;

public static class DriverMappingExtensions
{
    public static DriverDto ToDto(this Driver driver)
    {
        return new DriverDto
        {
            Id = driver.Id,
            OwnerUserId = driver.OwnerUserId,
            UserId = driver.UserId,

            FirstName = driver.FirstName,
            LastName = driver.LastName,
            DisplayName = driver.DisplayName,
            Email = driver.Email,
            PhoneNumber = driver.PhoneNumber,
            DateOfBirthUtc = driver.DateOfBirthUtc,

            LicenceNumber = driver.LicenceNumber,
            LicenceVersion = driver.LicenceVersion,
            LicenceClassOrEndorsements = driver.LicenceClassOrEndorsements,
            LicenceIssuedOnUtc = driver.LicenceIssuedOnUtc,
            LicenceExpiresOnUtc = driver.LicenceExpiresOnUtc,
            LicenceConditionsNotes = driver.LicenceConditionsNotes,

            Line1 = driver.Line1,
            Line2 = driver.Line2,
            Suburb = driver.Suburb,
            City = driver.City,
            Region = driver.Region,
            Postcode = driver.Postcode,
            Country = driver.Country,
            AddressSummary = driver.AddressSummary,

            Status = driver.Status.ToString(),

            EmergencyContact = new EmergencyContactDto
            {
                FirstName = driver.EmergencyContact?.FirstName,
                LastName = driver.EmergencyContact?.LastName,
                Relationship = driver.EmergencyContact?.Relationship,
                PhoneNumber = driver.EmergencyContact?.PhoneNumber,
                SecondaryPhoneNumber = driver.EmergencyContact?.SecondaryPhoneNumber,
                Email = driver.EmergencyContact?.Email
            },

            EmergencyStatus = driver.EmergencyStatus,
            IsMainProfile = driver.IsMainProfile
        };
    }
}