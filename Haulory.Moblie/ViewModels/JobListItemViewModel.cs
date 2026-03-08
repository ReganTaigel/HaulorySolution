using Haulory.Mobile.Contracts.Jobs;

namespace Haulory.Mobile.ViewModels;

public class JobListItemViewModel
{
    public JobDto Job { get; }

    public string DriverName { get; }
    public string TruckDisplay { get; }
    public bool ShowPricing { get; }

    public JobListItemViewModel(JobDto job, string driverName, string truckDisplay, bool showPricing)
    {
        Job = job;
        DriverName = driverName;
        TruckDisplay = truckDisplay;
        ShowPricing = showPricing;
    }

    public string ReferenceNumber => Job.ReferenceNumber;
    public string PickupCompany => Job.PickupCompany;
    public string PickupAddress => Job.PickupAddress;
    public string DeliveryCompany => Job.DeliveryCompany;
    public string DeliveryAddress => Job.DeliveryAddress;
    public string InvoiceNumber => Job.InvoiceNumber;
    public string Status => Job.Status;

    public decimal Total => Job.Total;

    public bool IsDelivered => Job.IsDelivered;
}