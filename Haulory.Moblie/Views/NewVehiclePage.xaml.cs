using Haulory.Mobile.ViewModels;
using System.Linq.Expressions;

namespace Haulory.Mobile.Views;

public partial class NewVehiclePage : ContentPage
{
	public NewVehiclePage(NewVehicleViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}