using Haulory.Moblie.ViewModels;

namespace Haulory.Moblie.Views;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
