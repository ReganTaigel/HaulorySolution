using Haulory.Api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Haulory.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SecureController : ControllerBase
{
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        return Ok(new
        {
            accountId = User.GetAccountUserId(),
            ownerId = User.GetOwnerUserId(),
            role = User.GetRoleName(),
            claims = User.Claims.Select(c => new { c.Type, c.Value })
        });
    }
}