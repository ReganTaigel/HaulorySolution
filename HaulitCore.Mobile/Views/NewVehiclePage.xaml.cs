using HaulitCore.Mobile.ViewModels;
using System.Linq.Expressions;

namespace HaulitCore.Mobile.Views;

public partial class NewVehiclePage : ContentPage
{
	public NewVehiclePage(NewVehicleViewModel vm)
	{
		InitializeComponent();

		BindingContext = vm;
	}
}