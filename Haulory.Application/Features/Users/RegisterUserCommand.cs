namespace Haulory.Application.Features.Users;

#region Command: Register User

public record RegisterUserCommand(

#region Basic Information

    // User first name
    string FirstName,

    // User last name
    string LastName,

    // User email (will be normalized in handler)
    string Email,

#endregion

#region Business Information 

    // NEW: Business (for Invoice + POD)
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

    // Plain-text password (must be hashed in handler)
    string Password

#endregion
);

#endregion
