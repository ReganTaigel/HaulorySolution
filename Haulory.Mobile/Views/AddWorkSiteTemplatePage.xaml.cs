using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class AddWorkSiteTemplatePage : ContentPage
{
    private readonly AddWorkSiteTemplateViewModel _vm;

    public AddWorkSiteTemplatePage(AddWorkSiteTemplateViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // If your app can restart, keep it consistent with your other pages
        await _vm.EnsureSessionAsync();
    }
}