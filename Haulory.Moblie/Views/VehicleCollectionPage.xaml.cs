using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class VehicleCollectionPage : ContentPage
{
    private readonly VehicleCollectionViewModel _vm;

    public VehicleCollectionPage(VehicleCollectionViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
