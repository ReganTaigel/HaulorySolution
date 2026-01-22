using Haulory.Moblie.ViewModels;

namespace Haulory.Moblie.Views;

public partial class VehicleCollectionPage : ContentPage
{
	public VehicleCollectionPage(VehicleCollectionViewModel vm)
	{
		InitializeComponent();

        BindingContext = vm;
    }
}