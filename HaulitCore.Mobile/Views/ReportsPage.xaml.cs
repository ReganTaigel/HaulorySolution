using HaulitCore.Mobile.ViewModels;

namespace HaulitCore.Mobile.Views;

public partial class ReportsPage : ContentPage
{
    public ReportsPage(ReportsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is ReportsViewModel vm)
            await vm.LoadAsync();
    }
}