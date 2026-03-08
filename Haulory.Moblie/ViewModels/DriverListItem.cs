using Haulory.Domain.Entities;
using Haulory.Mobile.Contracts.Drivers;

namespace Haulory.Mobile.ViewModels;

#region ViewModel Helper: Driver List Item

public class DriverListItem
{
    #region Data

    // Domain driver (used by legacy/local flows)
    public Driver? Driver { get; }

    // API driver (used by cloud flows)
    public DriverDto? DriverDto { get; }

    // Number of inductions expiring soon
    public int ExpiringSoonCount { get; }

    // Number of expired inductions
    public int ExpiredCount { get; }

    #endregion

    #region Constructors

    // Legacy/local constructor
    public DriverListItem(
        Driver driver,
        int expiringSoonCount,
        int expiredCount = 0)
    {
        Driver = driver;
        ExpiringSoonCount = expiringSoonCount;
        ExpiredCount = expiredCount;
    }

    // API constructor
    public DriverListItem(
        DriverDto driver,
        int expiringSoonCount = 0,
        int expiredCount = 0)
    {
        DriverDto = driver;
        ExpiringSoonCount = expiringSoonCount;
        ExpiredCount = expiredCount;
    }

    #endregion

    #region Derived Properties

    // Display name works for both domain and DTO
    public string DisplayName =>
        Driver?.DisplayName ??
        DriverDto?.DisplayName ??
        $"{DriverDto?.FirstName} {DriverDto?.LastName}".Trim();

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