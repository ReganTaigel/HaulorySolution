using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class VehicleCollectionPage : ContentPage
{
    private readonly VehicleCollectionViewModel _vm;

    public VehicleCollectionPage(VehicleCollectionViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _vm.LoadAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
        }
    }

}
