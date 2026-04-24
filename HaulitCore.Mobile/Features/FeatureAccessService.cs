using HaulitCore.Application.Interfaces.Services;

namespace HaulitCore.Mobile.Features;

public sealed class FeatureAccessService : IFeatureAccessService
{
    private readonly ISessionService _sessionService;

    public FeatureAccessService(ISessionService sessionService)
    {
        _sessionService = sessionService;
    }

    public bool IsEnabled(AppFeature feature) => GetAccess(feature).IsEnabled;

    public bool IsVisible(AppFeature feature) => GetAccess(feature).IsVisible;

    public FeatureAccess GetAccess(AppFeature feature)
    {
        return GetAccessInternal(feature, new HashSet<AppFeature>());
    }

    private FeatureAccess GetAccessInternal(AppFeature feature, HashSet<AppFeature> visited)
    {
        if (!visited.Add(feature))
            return FeatureAccess.Hidden();

        var directAccess = GetDirectAccess(feature);

        if (!directAccess.IsVisible)
            return directAccess;

        if (!FeatureDefinitions.Map.TryGetValue(feature, out var definition))
            return directAccess;

        if (definition.Parent.HasValue)
        {
            var parentAccess = GetAccessInternal(
                definition.Parent.Value,
                new HashSet<AppFeature>(visited));

            if (!parentAccess.IsVisible)
                return FeatureAccess.Hidden();

            if (!parentAccess.IsEnabled)
            {
                return FeatureAccess.Locked(
                    parentAccess.Message ?? "This feature is unavailable.");
            }
        }

        foreach (var dependency in definition.DependsOn)
        {
            if (dependency == feature)
                continue;

            var dependencyAccess = GetAccessInternal(
                dependency,
                new HashSet<AppFeature>(visited));

            if (!dependencyAccess.IsVisible)
                return FeatureAccess.Hidden();

            if (!dependencyAccess.IsEnabled)
            {
                return FeatureAccess.Locked(
                    dependencyAccess.Message ?? $"This feature depends on {dependency}.");
            }
        }

        return directAccess;
    }

    private FeatureAccess GetDirectAccess(AppFeature feature)
    {
        return feature switch
        {
            AppFeature.Dashboard => FeatureAccess.Enabled(),

            AppFeature.StartDay => FeatureAccess.Enabled(),
            AppFeature.EndDay => FeatureAccess.Enabled(),
            AppFeature.QuickStats => FeatureAccess.Enabled(),

            AppFeature.Jobs => FeatureAccess.Enabled(),
            AppFeature.AddJob => FeatureAccess.Enabled(),
            AppFeature.JobBilling => FeatureAccess.Enabled(),
            AppFeature.JobPickup => FeatureAccess.Enabled(),
            AppFeature.JobDelivery => FeatureAccess.Enabled(),
            AppFeature.JobLoadDetails => FeatureAccess.Enabled(),
            AppFeature.JobAssignment => FeatureAccess.Enabled(),
            AppFeature.DeliverySignature => FeatureAccess.Enabled(),

            AppFeature.Vehicles => FeatureAccess.Enabled(),
            AppFeature.AddVehicle => FeatureAccess.Enabled(),

            AppFeature.Drivers => FeatureAccess.Enabled(),
            AppFeature.AddDriver => FeatureAccess.Enabled(),
            AppFeature.Users => FeatureAccess.Enabled(),

            AppFeature.Inductions => FeatureAccess.Hidden(),

            AppFeature.Reports => FeatureAccess.Enabled(),
            AppFeature.ExportPod => FeatureAccess.Enabled(),
            AppFeature.ExportInvoice => FeatureAccess.Enabled(),

            _ => FeatureAccess.Enabled()
        };
    }
}