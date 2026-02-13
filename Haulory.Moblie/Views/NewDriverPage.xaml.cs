using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class NewDriverPage : ContentPage
{
    public NewDriverPage(NewDriverViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
