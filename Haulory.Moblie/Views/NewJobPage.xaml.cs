using Haulory.Moblie.ViewModels;

namespace Haulory.Moblie.Views;

public partial class NewJobPage : ContentPage
{
    public NewJobPage(NewJobViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
