using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class NewJobPage : ContentPage
{
    public NewJobPage(NewJobViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is NewJobViewModel vm)
            await vm.LoadAsync();
    }
}
