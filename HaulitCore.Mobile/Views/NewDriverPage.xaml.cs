using HaulitCore.Mobile.ViewModels;

namespace HaulitCore.Mobile.Views;

public partial class NewDriverPage : ContentPage
{
    private readonly NewDriverViewModel _viewModel;

    public NewDriverPage(NewDriverViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = _viewModel = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        try
        {
            await _viewModel.InitializeAsync();
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Load failed", ex.Message, "OK");
        }
    }
}