using Haulory.Application.Interfaces.Services;
using System;
using System.Collections.Generic;
using System.Text;

namespace Haulory.Infrastructure.Services
{
    public static class SessionExtensions
    {
        public static bool IsSubUser(this ISessionService s)
            => s.CurrentAccountId.HasValue
               && s.CurrentOwnerId.HasValue
               && s.CurrentOwnerId.Value != s.CurrentAccountId.Value;

        public static bool IsMainUser(this ISessionService s)
            => s.CurrentAccountId.HasValue
               && s.CurrentOwnerId.HasValue
               && s.CurrentOwnerId.Value == s.CurrentAccountId.Value;
    }
}
