using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class NeedsReviewPage : ContentPage
{
    private readonly NeedsReviewViewModel _viewModel;

    public NeedsReviewPage(NeedsReviewViewModel viewModel)
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