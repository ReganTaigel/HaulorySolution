using Haulory.Moblie.ViewModels;

namespace Haulory.Moblie.Views;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
