using HaulitCore.Mobile.Features;
using HaulitCore.Mobile.ViewModels;
using HaulitCore.Mobile.Views;

public class ExampleViewModel : BaseViewModel
{
    public ExampleViewModel(IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
    }

    public bool IsInductionsVisible => IsFeatureVisible(AppFeature.Inductions);

    public bool IsInductionsEnabled => IsFeatureEnabled(AppFeature.Inductions);

    public async Task GoToInductionsAsync()
    {
        if (!await EnsureFeatureEnabledAsync(AppFeature.Inductions))
            return;

        await Shell.Current.GoToAsync(nameof(ManageInductionsPage));
    }
}