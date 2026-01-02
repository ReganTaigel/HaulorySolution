using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Application.Features.Users
{
    public record LoginUserCommand(string Email, string Password);
}
