using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Application.Features.Drivers
{
    public record CreateDriverFromUserCommand(Guid UserId, string FirstName, string LastName, string Email);
}

