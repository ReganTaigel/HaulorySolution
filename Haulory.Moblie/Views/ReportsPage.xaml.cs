using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class ReportsPage : ContentPage
{
    private readonly ReportsViewModel _vm;

    public ReportsPage(ReportsViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}
