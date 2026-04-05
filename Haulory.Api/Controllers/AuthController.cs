using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Haulory.Contracts.Auth;
using Haulory.Application.Features.Drivers;
using Haulory.Application.Features.Users;
using Haulory.Application.Security;
using Haulory.Core.Security;
using Haulory.Domain.Entities;
using Haulory.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Haulory.Api.Controllers;

// Marks this class as an API controller and enables automatic model binding/validation behavior.
[ApiController]

// Sets the base route for this controller to: api/auth
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    // Database context used to access user and driver data.
    private readonly HauloryDbContext _db;

    // Application configuration used here to read JWT settings.
    private readonly IConfiguration _configuration;

    // Handler responsible for creating a Driver record from a UserAccount.
    private readonly CreateDriverFromUserHandler _createDriverFromUserHandler;

    // Constructor injection for required services.
    public AuthController(
        HauloryDbContext db,
        IConfiguration configuration,
        CreateDriverFromUserHandler createDriverFromUserHandler)
    {
        _db = db;
        _configuration = configuration;
        _createDriverFromUserHandler = createDriverFromUserHandler;
    }

    // Registers a new user account.
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand request)
    {
        // Normalise the email to avoid duplicates caused by casing or whitespace differences.
        var email = request.Email.Trim().ToLowerInvariant();

        // Check whether an account with the same email already exists.
        var exists = await _db.UserAccounts.AnyAsync(x => x.Email == email);
        if (exists)
            return Conflict("An account with that email already exists.");

        // Validate the password against the application's password policy.
        if (!PasswordPolicy.IsValid(request.Password, out var errorMessage))
            return BadRequest(errorMessage);

        // Hash the password before storing it in the database.
        var passwordHash = PasswordHasher.Hash(request.Password);

        // Create the new user account entity.
        var user = new UserAccount(
            request.FirstName,
            request.LastName,
            email,
            passwordHash);

        // Store the business identity details for the account.
        user.UpdateBusinessIdentity(
            request.BusinessName,
            request.BusinessEmail,
            request.SupplierGstNumber,
            request.SupplierNzbn);

        // Store the business contact phone number.
        user.UpdateBusinessContact(request.BusinessPhone);

        // Store the business address details.
        user.UpdateBusinessAddress(
            request.BusinessAddress1,
            request.BusinessAddress2,
            request.BusinessSuburb,
            request.BusinessCity,
            request.BusinessRegion,
            request.BusinessPostcode,
            request.BusinessCountry);

        // Add the new user to the database and save it.
        _db.UserAccounts.Add(user);
        await _db.SaveChangesAsync();

        // Automatically create a related Driver record for the new user.
        // Most driver-specific fields are initially null and can be completed later.
        await _createDriverFromUserHandler.HandleAsync(
            new CreateDriverFromUserCommand(
                UserId: user.Id,
                FirstName: user.FirstName,
                LastName: user.LastName,
                Email: user.Email,

                PhoneNumber: null,
                DateOfBirthUtc: null,

                LicenceNumber: null,
                LicenceVersion: null,
                LicenceClassOrEndorsements: null,
                LicenceIssuedOnUtc: null,
                LicenceExpiresOnUtc: null,
                LicenceConditionsNotes: null,

                Line1: null,
                Line2: null,
                Suburb: null,
                City: null,
                Region: null,
                Postcode: null,
                Country: null
            ));

        // Return key account details after successful registration.
        return Ok(new
        {
            accountId = user.Id,
            ownerId = user.OwnerUserId,
            email = user.Email,
            role = user.Role.ToString()
        });
    }

    // Authenticates an existing user and returns a JWT token if successful.
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        // Normalise the email before querying the database.
        var email = request.Email.Trim().ToLowerInvariant();

        // Find the user account that matches the provided email.
        var user = await _db.UserAccounts
            .SingleOrDefaultAsync(x => x.Email == email);

        // Reject the login if the user does not exist or the password is incorrect.
        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        // Check whether this user already has a related Driver record.
        var existingDriver = await _db.Drivers
            .FirstOrDefaultAsync(x => x.UserId == user.Id);

        // If no Driver record exists yet, create one automatically.
        if (existingDriver == null)
        {
            await _createDriverFromUserHandler.HandleAsync(
                new CreateDriverFromUserCommand(
                    UserId: user.Id,
                    FirstName: user.FirstName,
                    LastName: user.LastName,
                    Email: user.Email,

                    PhoneNumber: null,
                    DateOfBirthUtc: null,

                    LicenceNumber: null,
                    LicenceVersion: null,
                    LicenceClassOrEndorsements: null,
                    LicenceIssuedOnUtc: null,
                    LicenceExpiresOnUtc: null,
                    LicenceConditionsNotes: null,

                    Line1: null,
                    Line2: null,
                    Suburb: null,
                    City: null,
                    Region: null,
                    Postcode: null,
                    Country: null
                ));
        }

        // OwnerUserId is used to group related records under the owning account.
        var ownerId = user.OwnerUserId;

        // Read JWT configuration values from app settings.
        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;

        // Build the claims that will be embedded in the JWT.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("account_id", user.Id.ToString()),
            new("owner_id", ownerId.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        // Create signing credentials using the configured symmetric key.
        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        // Create the JWT token with issuer, audience, claims, expiry, and signature.
        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        // Serialize the token into a string that can be returned to the client.
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        // Return the login response including the token and account information.
        return Ok(new LoginResponse
        {
            Token = jwt,
            AccountId = user.Id,
            OwnerId = ownerId,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Role = user.Role.ToString()
        });
    }
}