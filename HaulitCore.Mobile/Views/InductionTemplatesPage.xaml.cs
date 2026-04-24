using HaulitCore.Mobile.ViewModels;

namespace HaulitCore.Mobile.Views;

public partial class InductionTemplatesPage : ContentPage
{
	public InductionTemplatesPage(InductionTemplatesViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
	}
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (BindingContext is InductionTemplatesViewModel vm)
            await vm.LoadAsync();
    }

}