using Haulory.Application.Features.Jobs;
using Haulory.Domain.Enums;
using Haulory.Mobile.ViewModels;
using Haulory.Mobile.Views;
using System.Windows.Input;

public class NewJobViewModel : BaseViewModel
{
    private readonly CreateJobHandler _handler;

    public IReadOnlyList<RateType> RateTypes { get; } =
        Enum.GetValues(typeof(RateType)).Cast<RateType>().ToList();

    public string PickupCompany { get; set; }
    public string PickupAddress { get; set; }
    public string DeliveryCompany { get; set; }
    public string DeliveryAddress { get; set; }
    public string ReferenceNumber { get; set; }
    public string LoadDescription { get; set; }

    public RateType RateType { get; set; }

    private int _quantity;
    public int Quantity
    {
        get => _quantity;
        set
        {
            if (_quantity == value) return;
            _quantity = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Total));
        }
    }

    private decimal _rateValue;
    public decimal RateValue
    {
        get => _rateValue;
        set
        {
            if (_rateValue == value) return;
            _rateValue = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(Total));
        }
    }

    public decimal Total => RateValue * Quantity;

    public ICommand SaveJobCommand { get; }
    public ICommand CancelCommand { get; }

    public NewJobViewModel(CreateJobHandler handler)
    {
        _handler = handler;

        SaveJobCommand = new Command(async () => await SaveAsync());
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(DashboardPage)));
    }


    private async Task SaveAsync()
    {
        await _handler.HandleAsync(new CreateJobCommand(
            PickupCompany,
            PickupAddress,
            DeliveryCompany,
            DeliveryAddress,
            ReferenceNumber,
            LoadDescription,
            RateType,
            RateValue,
            Quantity));

        await Shell.Current.GoToAsync(nameof(JobsCollectionPage));
    }
}

