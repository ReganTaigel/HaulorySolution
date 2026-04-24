namespace HaulitCore.Mobile.Features;

public interface IFeatureAccessService
{
    FeatureAccess GetAccess(AppFeature feature);
    bool IsEnabled(AppFeature feature);
    bool IsVisible(AppFeature feature);
}