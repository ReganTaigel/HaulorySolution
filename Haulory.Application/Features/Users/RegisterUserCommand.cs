namespace Haulory.Application.Features.Users;

#region Command: Register User

public record RegisterUserCommand(

#region Basic Information

    string FirstName,
    string LastName,
    string Email,

#endregion

#region Business Information

    string BusinessName,
    string? BusinessEmail,
    string? BusinessPhone,
    string? BusinessAddress1,
    string? BusinessAddress2,
    string? BusinessSuburb,
    string? BusinessCity,
    string? BusinessRegion,
    string? BusinessPostcode,
    string? BusinessCountry,
    string? SupplierGstNumber,
    string? SupplierNzbn,

#endregion

#region Credentials

    string Password

#endregion

);

#endregion