using Haulory.Moblie.ViewModels;
using System.Linq.Expressions;

namespace Haulory.Moblie.Views;

public partial class NewVehiclePage : ContentPage
{
	public NewVehiclePage(NewVehicleViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}