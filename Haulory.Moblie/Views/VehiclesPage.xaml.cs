using Haulory.Moblie.ViewModels;
using System.Linq.Expressions;

namespace Haulory.Moblie.Views;

public partial class VehiclesPage : ContentPage
{
	public VehiclesPage(VehicleViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}