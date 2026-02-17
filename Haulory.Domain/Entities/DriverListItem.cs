using Haulory.Domain.Entities;

namespace Haulory.Mobile.ViewModels;

public class DriverListItem
{
    public Driver Driver { get; }

    public int ExpiringSoonCount { get; }
    public int ExpiredCount { get; }

    public DriverListItem(
        Driver driver,
        int expiringSoonCount,
        int expiredCount = 0)
    {
        Driver = driver;
        ExpiringSoonCount = expiringSoonCount;
        ExpiredCount = expiredCount;
    }

    public bool HasWarnings => ExpiringSoonCount > 0 || ExpiredCount > 0;
}
