using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Haulory.Mobile.Features;

namespace Haulory.Mobile.ViewModels;

public abstract class BaseViewModel : INotifyPropertyChanged
{
    protected readonly IFeatureAccessService? FeatureAccessService;

    protected BaseViewModel()
    {
    }

    protected BaseViewModel(IFeatureAccessService featureAccessService)
    {
        FeatureAccessService = featureAccessService;
    }

    #region INotifyPropertyChanged

    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Property Helpers

    protected bool SetProperty<T>(
        ref T backingStore,
        T value,
        [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    protected void OnPropertyChanged(
        [CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(
            this,
            new PropertyChangedEventArgs(propertyName));
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (_isBusy == value)
                return;

            _isBusy = value;
            OnPropertyChanged();
        }
    }

    #endregion

    #region Feature Access Helpers

    protected bool IsFeatureVisible(AppFeature feature)
    {
        var result = FeatureAccessService?.IsVisible(feature) ?? true;
        System.Diagnostics.Debug.WriteLine($"[IsFeatureVisible] {feature} => {result}");
        return result;
    }

    protected bool IsFeatureEnabled(AppFeature feature)
    {
        var result = FeatureAccessService?.IsEnabled(feature) ?? true;
        System.Diagnostics.Debug.WriteLine($"[IsFeatureEnabled] {feature} => {result}");
        return result;
    }

    protected async Task<bool> EnsureFeatureEnabledAsync(AppFeature feature)
    {
        if (FeatureAccessService is null)
            return true;

        var access = FeatureAccessService.GetAccess(feature);

        if (access.IsEnabled)
            return true;

        await Shell.Current.DisplayAlertAsync(
            "Unavailable",
            access.Message ?? "This feature is unavailable.",
            "OK");

        return false;
    }

    #endregion
}