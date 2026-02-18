using Haulory.Domain.Entities;

namespace Haulory.Mobile.ViewModels;

#region ViewModel Helper: Driver List Item

public class DriverListItem
{
    #region Data

    // Underlying domain driver
    public Driver Driver { get; }

    // Number of inductions expiring soon
    public int ExpiringSoonCount { get; }

    // Number of expired inductions
    public int ExpiredCount { get; }

    #endregion

    #region Constructor

    public DriverListItem(
        Driver driver,
        int expiringSoonCount,
        int expiredCount = 0)
    {
        Driver = driver;
        ExpiringSoonCount = expiringSoonCount;
        ExpiredCount = expiredCount;
    }

    #endregion

    #region Derived Properties

    // Full display name shortcut
    public string DisplayName => Driver.DisplayName;

    // True if driver has any compliance warnings
    public bool HasWarnings =>
        ExpiringSoonCount > 0 || ExpiredCount > 0;

    // Warning summary text for UI badges
    public string WarningSummary
    {
        get
        {
            if (!HasWarnings)
                return string.Empty;

            if (ExpiredCount > 0 && ExpiringSoonCount > 0)
                return $"{ExpiredCount} expired • {ExpiringSoonCount} due soon";

            if (ExpiredCount > 0)
                return $"{ExpiredCount} expired";

            return $"{ExpiringSoonCount} due soon";
        }
    }

    #endregion
}

#endregion
