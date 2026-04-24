using HaulitCore.Contracts.Jobs;

namespace HaulitCore.Mobile.ViewModels;

public class JobListItemViewModel
{
    public JobDto Job { get; }

    public string DriverName { get; }
    public string TruckDisplay { get; }
    public string TrailerDisplay { get; }
    public bool ShowPricing { get; }
    public bool CanShowSignDelivery { get; }

    public JobListItemViewModel(
        JobDto job,
        string driverName,
        string truckDisplay,
        string trailerDisplay,
        bool showPricing,
        bool canShowSignDelivery)
    {
        Job = job;
        DriverName = driverName;
        TruckDisplay = truckDisplay;
        TrailerDisplay = string.IsNullOrWhiteSpace(trailerDisplay) ? "—" : trailerDisplay;
        ShowPricing = showPricing;
        CanShowSignDelivery = canShowSignDelivery;
    }

    public string ReferenceNumber => Job.ReferenceNumber;
    public string PickupCompany => Job.PickupCompany;
    public string PickupAddress => Job.PickupAddress;
    public string DeliveryCompany => Job.DeliveryCompany;
    public string DeliveryAddress => Job.DeliveryAddress;
    public string InvoiceNumber => Job.InvoiceNumber;
    public string Status => Job.Status;
    public string LoadDescription => Job.LoadDescription;

    public decimal Total => Job.Total;
    public bool IsDelivered => Job.IsDelivered;

    public string RateLine
    {
        get
        {
            var rateType = string.IsNullOrWhiteSpace(Job.RateType) ? "Rate" : Job.RateType;
            return $"Rate: {rateType} @ {Job.RateValue:N2} × {Job.Quantity:N2}";
        }
    }

    public string TotalLine => $"Total: {Job.Total:N2}";
}