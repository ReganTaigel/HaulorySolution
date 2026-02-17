using Haulory.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

public static class InductionRules
{
    public static bool IsExpiringWithinDays(DriverInduction x, DateTime utcNow, int days)
    {
        if (!x.ExpiresOnUtc.HasValue) return false;
        return x.ExpiresOnUtc.Value > utcNow &&
               x.ExpiresOnUtc.Value <= utcNow.AddDays(days);
    }
}