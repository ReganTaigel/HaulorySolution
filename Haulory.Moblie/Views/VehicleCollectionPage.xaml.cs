using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class VehicleCollectionPage : ContentPage
{
    public VehicleCollectionPage(VehicleCollectionViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        System.Diagnostics.Debug.WriteLine("[VehicleCollectionPage] OnAppearing fired");

        if (BindingContext is VehicleCollectionViewModel vm)
            await vm.LoadAsync();
    }
}