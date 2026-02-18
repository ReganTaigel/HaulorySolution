using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Mobile.ViewModels;

#region ViewModel Helper: Job List Item

public class JobListItemViewModel
{
    #region Constructor

    public JobListItemViewModel(Job job, string driverName, string truckDisplay)
    {
        Job = job;

        // Fallback display values
        DriverName = string.IsNullOrWhiteSpace(driverName) ? "—" : driverName;
        TruckDisplay = string.IsNullOrWhiteSpace(truckDisplay) ? "—" : truckDisplay;
    }

    #endregion

    #region Underlying Domain Model

    public Job Job { get; }

    #endregion

    #region Pass-Through Properties (Simplifies XAML)

    public Guid Id => Job.Id;
    public string ReferenceNumber => Job.ReferenceNumber;

    public string PickupCompany => Job.PickupCompany;
    public string PickupAddress => Job.PickupAddress;

    public string DeliveryCompany => Job.DeliveryCompany;
    public string DeliveryAddress => Job.DeliveryAddress;

    public string LoadDescription => Job.LoadDescription;
    public string InvoiceNumber => Job.InvoiceNumber;

    public bool IsDelivered => Job.IsDelivered;

    #endregion

    #region Assignment Display

    public string DriverName { get; }
    public string TruckDisplay { get; }

    #endregion

    #region Display Helpers

    // Friendly rate description line
    public string RateLine => Job.RateType switch
    {
        RateType.FixedFee =>
            $"Rate: Fixed ${Job.RateValue:0.##}",

        RateType.PerPallet =>
            $"Rate: ${Job.RateValue:0.##} / pallet × {Job.Quantity:0.##}",

        RateType.PerTonne =>
            $"Rate: ${Job.RateValue:0.##} / tonne × {Job.Quantity:0.##}",

        RateType.PerKm =>
            $"Rate: ${Job.RateValue:0.##} / km × {Job.Quantity:0.##}",

        RateType.Hourly =>
            $"Rate: ${Job.RateValue:0.##} / hr × {Job.Quantity:0.##}",

        RateType.PerLoad =>
            $"Rate: ${Job.RateValue:0.##} / load × {Job.Quantity:0.##}",

        RateType.Percentage =>
            $"Rate: {Job.RateValue:0.##}% (qty fixed)",

        _ =>
            $"Rate: ${Job.RateValue:0.##} × {Job.Quantity:0.##}"
    };

    public string TotalLine =>
        $"Total: ${Job.Total:0.##}";

    // Combined card display summary
    public string CardSummary =>
        $"{PickupCompany} → {DeliveryCompany}\n" +
        $"{LoadDescription}\n" +
        $"{RateLine} • {TotalLine}";

    #endregion
}

#endregion
