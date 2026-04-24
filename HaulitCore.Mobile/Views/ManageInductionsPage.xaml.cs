using HaulitCore.Mobile.ViewModels;

namespace HaulitCore.Mobile.Views;

public partial class ManageInductionsPage : ContentPage
{
    private readonly ManageInductionsViewModel _vm;

    public ManageInductionsPage(ManageInductionsViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadAsync();
    }
}