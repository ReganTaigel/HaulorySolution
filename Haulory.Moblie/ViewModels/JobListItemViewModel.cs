using Haulory.Domain.Entities;
using Haulory.Domain.Enums;

namespace Haulory.Mobile.ViewModels;

public class JobListItemViewModel
{
    public JobListItemViewModel(Job job, string driverName, string truckDisplay)
    {
        Job = job;
        DriverName = string.IsNullOrWhiteSpace(driverName) ? "—" : driverName;
        TruckDisplay = string.IsNullOrWhiteSpace(truckDisplay) ? "—" : truckDisplay;
    }

    public Job Job { get; }

    // pass-throughs (so XAML stays simple)
    public Guid Id => Job.Id;
    public string ReferenceNumber => Job.ReferenceNumber;

    public string PickupCompany => Job.PickupCompany;
    public string PickupAddress => Job.PickupAddress;

    public string DeliveryCompany => Job.DeliveryCompany;
    public string DeliveryAddress => Job.DeliveryAddress;

    public string LoadDescription => Job.LoadDescription;
    public string InvoiceNumber => Job.InvoiceNumber;

    public bool IsDelivered => Job.IsDelivered;

    public string DriverName { get; }
    public string TruckDisplay { get; }

    // Nice card lines
    public string RateLine => Job.RateType switch
    {
        RateType.FixedFee => $"Rate: Fixed ${Job.RateValue:0.##}",
        RateType.PerPallet => $"Rate: ${Job.RateValue:0.##} / pallet × {Job.Quantity:0.##}",
        RateType.PerTonne => $"Rate: ${Job.RateValue:0.##} / tonne × {Job.Quantity:0.##}",
        RateType.PerKm => $"Rate: ${Job.RateValue:0.##} / km × {Job.Quantity:0.##}",
        RateType.Hourly => $"Rate: ${Job.RateValue:0.##} / hr × {Job.Quantity:0.##}",
        RateType.PerLoad => $"Rate: ${Job.RateValue:0.##} / load × {Job.Quantity:0.##}",
        RateType.Percentage => $"Rate: {Job.RateValue:0.##}% (qty fixed)",
        _ => $"Rate: ${Job.RateValue:0.##} × {Job.Quantity:0.##}"
    };

    public string TotalLine => $"Total: ${Job.Total:0.##}";

    public string CardSummary =>
        $"{PickupCompany} → {DeliveryCompany}\n{LoadDescription}\n{RateLine} • {TotalLine}";
}
