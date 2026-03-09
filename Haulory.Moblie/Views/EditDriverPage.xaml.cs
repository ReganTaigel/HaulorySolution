using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

[QueryProperty(nameof(DriverId), "driverId")]
public partial class EditDriverPage : ContentPage
{
    private readonly EditDriverViewModel _viewModel;

    private string _driverId = string.Empty;
    public string DriverId
    {
        get => _driverId;
        set
        {
            _driverId = Uri.UnescapeDataString(value ?? string.Empty);
            _ = _viewModel.InitializeAsync(_driverId);
        }
    }

    public EditDriverPage(EditDriverViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }
}