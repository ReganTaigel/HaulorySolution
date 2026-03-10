namespace Haulory.Mobile.Features;

public sealed class FeatureAccess
{
    public FeatureAvailability Availability { get; init; }
    public string? Message { get; init; }

    public bool IsEnabled => Availability == FeatureAvailability.Enabled;
    public bool IsVisible => Availability != FeatureAvailability.Hidden;
    public bool IsLocked => Availability == FeatureAvailability.Locked;
    public bool IsHidden => Availability == FeatureAvailability.Hidden;

    public static FeatureAccess Enabled() =>
        new() { Availability = FeatureAvailability.Enabled };

    public static FeatureAccess Hidden() =>
        new() { Availability = FeatureAvailability.Hidden };

    public static FeatureAccess Locked(string message) =>
        new() { Availability = FeatureAvailability.Locked, Message = message };
}