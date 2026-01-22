using Haulory.Moblie.Views;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Haulory.Moblie.ViewModels
{
    public class VehicleCollectionViewModel
    {
        #region Commands

        public ICommand GoToNewVehicleCommand { get;}

        #endregion

        #region Constructor

        public VehicleCollectionViewModel()
        {
            GoToNewVehicleCommand = new Command(async () => await Shell.Current.GoToAsync(nameof(NewVehiclePage)));
        }
        #endregion
    }
}
