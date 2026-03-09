using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class NewJobPage : ContentPage
{
    private readonly NewJobViewModel _viewModel;

    public NewJobPage(NewJobViewModel viewModel)
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