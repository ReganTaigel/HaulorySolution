using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class OdometerEntryPage : ContentPage
{
    private readonly OdometerEntryViewModel _viewModel;

    public OdometerEntryPage(OdometerEntryViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}