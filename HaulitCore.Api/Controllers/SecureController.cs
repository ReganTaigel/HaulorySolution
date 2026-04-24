using HaulitCore.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HaulitCore.Api.Controllers;

// Marks this as an API controller with automatic binding and validation behavior.
[ApiController]

// Base route: api/secure
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    // Returns information about the currently authenticated user.
    // Useful for debugging authentication, JWT contents, and claims mapping.
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            // The account ID (typically the signed-in user).
            accountId = User.GetAccountUserId(),

            // The owner ID (used for tenant/business scoping).
            ownerId = User.GetOwnerUserId(),

            // The role assigned to the user (e.g., Admin, Driver).
            role = User.GetRoleName(),

            // All raw claims contained in the JWT for inspection/debugging.
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}