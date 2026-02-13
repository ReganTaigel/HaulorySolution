using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

[QueryProperty(nameof(DriverId), "driverId")]
public partial class EditDriverPage : ContentPage
{
    private readonly EditDriverViewModel _vm;

    public EditDriverPage(EditDriverViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    private string _driverId = string.Empty;
    public string DriverId
    {
        get => _driverId;
        set => _driverId = value;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!string.IsNullOrWhiteSpace(_driverId))
            await _vm.InitializeAsync(_driverId);
    }
}
