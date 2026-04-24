using HaulitCore.Mobile.ViewModels;

namespace HaulitCore.Mobile.Views;

public partial class HubodometerEntryPage : ContentPage
{
    private readonly HubodometerEntryViewModel _viewModel;

    public HubodometerEntryPage(HubodometerEntryViewModel viewModel)
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