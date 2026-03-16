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

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly HauloryDbContext _db;
    private readonly IConfiguration _configuration;
    private readonly CreateDriverFromUserHandler _createDriverFromUserHandler;

    public AuthController(
        HauloryDbContext db,
        IConfiguration configuration,
        CreateDriverFromUserHandler createDriverFromUserHandler)
    {
        _db = db;
        _configuration = configuration;
        _createDriverFromUserHandler = createDriverFromUserHandler;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterUserCommand request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var exists = await _db.UserAccounts.AnyAsync(x => x.Email == email);
        if (exists)
            return Conflict("An account with that email already exists.");

        if (!PasswordPolicy.IsValid(request.Password, out var errorMessage))
            return BadRequest(errorMessage);

        var passwordHash = PasswordHasher.Hash(request.Password);

        var user = new UserAccount(
            request.FirstName,
            request.LastName,
            email,
            passwordHash);

        user.UpdateBusinessIdentity(
            request.BusinessName,
            request.BusinessEmail,
            request.SupplierGstNumber,
            request.SupplierNzbn);

        user.UpdateBusinessContact(request.BusinessPhone);

        user.UpdateBusinessAddress(
            request.BusinessAddress1,
            request.BusinessAddress2,
            request.BusinessSuburb,
            request.BusinessCity,
            request.BusinessRegion,
            request.BusinessPostcode,
            request.BusinessCountry);

        _db.UserAccounts.Add(user);
        await _db.SaveChangesAsync();

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

        return Ok(new
        {
            accountId = user.Id,
            ownerId = user.OwnerUserId,
            email = user.Email,
            role = user.Role.ToString()
        });
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.UserAccounts
            .SingleOrDefaultAsync(x => x.Email == email);

        if (user is null || !PasswordHasher.Verify(request.Password, user.PasswordHash))
            return Unauthorized("Invalid email or password.");

        var existingDriver = await _db.Drivers
            .FirstOrDefaultAsync(x => x.UserId == user.Id);

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

        var ownerId = user.OwnerUserId;

        var jwtKey = _configuration["Jwt:Key"]!;
        var jwtIssuer = _configuration["Jwt:Issuer"]!;
        var jwtAudience = _configuration["Jwt:Audience"]!;

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new("account_id", user.Id.ToString()),
            new("owner_id", ownerId.ToString()),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            claims: claims,
            expires: DateTime.UtcNow.AddDays(7),
            signingCredentials: credentials);

        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

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