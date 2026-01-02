using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Application.Features.Users
{
    public record RegisterUserCommand
    (string FirstName, string LastName, string Email, string Password);
}
