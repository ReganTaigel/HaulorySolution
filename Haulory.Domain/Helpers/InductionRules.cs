using System;
using Haulory.Domain.Entities;

namespace Haulory.Domain.Rules;

#region Static Class: Induction Rules

// Domain rule helpers for DriverInduction
public static class InductionRules
{
    // Returns true if induction expires within the next X days
    public static bool IsExpiringWithinDays(
        DriverInduction x,
        DateTime utcNow,
        int days)
    {
        if (!x.ExpiresOnUtc.HasValue)
            return false;

        return x.ExpiresOnUtc.Value > utcNow &&
               x.ExpiresOnUtc.Value <= utcNow.AddDays(days);
    }
}

#endregion
