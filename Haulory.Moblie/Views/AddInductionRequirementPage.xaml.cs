using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.Views;

public partial class AddInductionRequirementPage : ContentPage
{
    public AddInductionRequirementPage(AddInductionRequirementViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
