using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class AddWorkSitePage : ContentPage
{
    public AddWorkSitePage(AddWorkSiteViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
