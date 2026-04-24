using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Mobile.Features;

namespace HaulitCore.Mobile.ViewModels;

public partial class AppShellViewModel : BaseViewModel
{
    private readonly ISessionService _sessionService;

    public AppShellViewModel(
        ISessionService sessionService,
        IFeatureAccessService featureAccessService)
        : base(featureAccessService)
    {
        _sessionService = sessionService;
    }

    public bool IsSubUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentOwnerId.Value != _sessionService.CurrentAccountId.Value;

    public bool IsMainUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentOwnerId.Value == _sessionService.CurrentAccountId.Value;

    public bool CanSeeDrivers => IsMainUser && IsFeatureVisible(AppFeature.Drivers);
    public bool CanSeeReports => IsMainUser && IsFeatureVisible(AppFeature.Reports);
    public bool CanSeeSettings => IsMainUser;
}