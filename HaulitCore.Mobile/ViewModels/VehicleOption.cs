namespace HaulitCore.Mobile.ViewModels;

// Generic wrapper used for picker options.
// Separates the stored enum/value (Value) from the user-friendly text (Display). 
// Example:
// new VehicleOption&lt;VehicleType&gt;(VehicleType.TruckClass2, "Truck (Class 2)")
public class VehicleOption<T>
{
    #region Properties

    // The actual enum/value used in business logic and persistence.
    public T Value { get; }

    // The display text shown in UI pickers.
    public string Display { get; }

    #endregion

    #region Constructor

    public VehicleOption(T value, string display)
    {
        Value = value;
        Display = display;
    }

    #endregion

    #region Overrides

    // Ensures the Picker shows Display text automatically.
    public override string ToString() => Display;

    #endregion
}
