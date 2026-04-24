using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Mobile.Features;
using HaulitCore.Mobile.Views;
using System;
using System.Diagnostics;
using System.Linq;

namespace HaulitCore.Mobile;

public partial class AppShell : Shell
{
    public const string RouteDashboard = nameof(DashboardPage);
    public const string RouteLogin = nameof(LoginPage);
    public const string RouteRegister = nameof(RegisterPage);
    public const string RouteSettings = nameof(SettingsPage);

    public const string RouteJobs = nameof(JobsCollectionPage);
    public const string RouteVehicles = nameof(VehicleCollectionPage);
    public const string RouteDrivers = nameof(DriverCollectionPage);
    public const string RouteReports = nameof(ReportsPage);
    public const string RouteNeedsReview = nameof(NeedsReviewPage);

    private readonly ISessionService _sessionService;
    private readonly IFeatureAccessService _featureAccessService;

    private bool IsMainUser =>
        _sessionService.CurrentAccountId.HasValue &&
        _sessionService.CurrentOwnerId.HasValue &&
        _sessionService.CurrentAccountId.Value == _sessionService.CurrentOwnerId.Value;

    public AppShell(
        ISessionService sessionService,
        IFeatureAccessService featureAccessService)
    {
        InitializeComponent();

        _sessionService = sessionService;
        _featureAccessService = featureAccessService;

        RegisterRoutes();

        Navigating += OnNavigating;
    }

    public Task GoHomeAsync() => GoToAsync($"//{RouteDashboard}");

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await HandleLogoutAsync();
    }

    private async void OnLogoutClicked(object? sender, TappedEventArgs e)
    {
        await HandleLogoutAsync();
    }

    private async Task HandleLogoutAsync()
    {
        try
        {
            bool confirm = await Current.DisplayAlertAsync(
                "Logout",
                "Are you sure you want to sign out?",
                "Yes",
                "No");

            if (!confirm)
                return;

            await _sessionService.ClearAsync();
            FlyoutIsPresented = false;
            await GoToAsync($"//{RouteLogin}");
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private void ApplyRoleVisibility()
    {
        bool isSubUser =
            _sessionService.CurrentAccountId.HasValue &&
            _sessionService.CurrentOwnerId.HasValue &&
            _sessionService.CurrentAccountId.Value != _sessionService.CurrentOwnerId.Value;

        DriversFlyoutItem.IsVisible = !isSubUser;
        ReportsFlyoutItem.IsVisible = !isSubUser;
        NeedsReviewFlyoutItem.IsVisible = !isSubUser;
        SettingsFlyoutItem.IsVisible = !isSubUser;
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(NewJobPage), typeof(NewJobPage));
        Routing.RegisterRoute(nameof(DeliverySignaturePage), typeof(DeliverySignaturePage));
        Routing.RegisterRoute(nameof(NewVehiclePage), typeof(NewVehiclePage));
        Routing.RegisterRoute(nameof(NewDriverPage), typeof(NewDriverPage));
        Routing.RegisterRoute(nameof(ManageInductionsPage), typeof(ManageInductionsPage));
        Routing.RegisterRoute(nameof(InductionTemplatesPage), typeof(InductionTemplatesPage));
        Routing.RegisterRoute(nameof(AddWorkSiteTemplatePage), typeof(AddWorkSiteTemplatePage));
    }

    private static bool IsRoute(string target, string route) =>
        target.Contains(route, StringComparison.OrdinalIgnoreCase);

    private static readonly string[] ProtectedRoutes =
    {
        RouteDashboard,
        RouteJobs,
        RouteVehicles,
        RouteDrivers,
        RouteReports,
        RouteNeedsReview,
        RouteSettings,
        nameof(ManageInductionsPage),
        nameof(InductionTemplatesPage),
        nameof(AddWorkSiteTemplatePage),
    };

    private static readonly string[] AuthRoutes =
    {
        RouteLogin,
        RouteRegister
    };

    private static bool IsProtectedRoute(string target) =>
        ProtectedRoutes.Any(r => target.Contains(r, StringComparison.OrdinalIgnoreCase));

    private static bool IsAuthRoute(string target) =>
        AuthRoutes.Any(r => target.Contains(r, StringComparison.OrdinalIgnoreCase));

    private FeatureAccess? GetFeatureAccessForRoute(string target)
    {
        if (target.Contains(nameof(ManageInductionsPage), StringComparison.OrdinalIgnoreCase) ||
            target.Contains(nameof(InductionTemplatesPage), StringComparison.OrdinalIgnoreCase) ||
            target.Contains(nameof(AddWorkSiteTemplatePage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Inductions);
        }

        if (target.Contains(RouteDrivers, StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Drivers);
        }

        if (target.Contains(nameof(NewDriverPage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.AddDriver);
        }

        if (target.Contains(RouteJobs, StringComparison.OrdinalIgnoreCase) ||
            target.Contains(nameof(DeliverySignaturePage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Jobs);
        }

        if (target.Contains(nameof(NewJobPage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.AddJob);
        }

        if (target.Contains(RouteVehicles, StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Vehicles);
        }

        if (target.Contains(nameof(NewVehiclePage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.AddVehicle);
        }

        if (target.Contains(RouteReports, StringComparison.OrdinalIgnoreCase) ||
            target.Contains(RouteNeedsReview, StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Reports);
        }

        if (target.Contains(RouteSettings, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return null;
    }

    private async void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        try
        {
            if (!_sessionService.IsAuthenticated)
                await _sessionService.RestoreAsync();

            ApplyRoleVisibility();

            var target = e.Target?.Location?.OriginalString ?? string.Empty;

            if (string.IsNullOrWhiteSpace(target))
                return;

            var featureAccess = GetFeatureAccessForRoute(target);
            if (featureAccess is not null && !featureAccess.IsEnabled)
            {
                e.Cancel();

                await Current.DisplayAlertAsync(
                    "Unavailable",
                    featureAccess.Message ?? "This feature is unavailable.",
                    "OK");

                return;
            }

            if (!_sessionService.IsAuthenticated && IsProtectedRoute(target))
            {
                if (!IsRoute(target, RouteLogin))
                {
                    e.Cancel();
                    await GoToAsync($"//{RouteLogin}");
                }

                return;
            }

            if (_sessionService.IsAuthenticated && !IsMainUser)
            {
                if (target.Contains(RouteDrivers, StringComparison.OrdinalIgnoreCase) ||
                    target.Contains(RouteReports, StringComparison.OrdinalIgnoreCase) ||
                    target.Contains(RouteNeedsReview, StringComparison.OrdinalIgnoreCase) ||
                    target.Contains(RouteSettings, StringComparison.OrdinalIgnoreCase))
                {
                    e.Cancel();

                    await Current.DisplayAlertAsync(
                        "Access denied",
                        "Only the account owner can access this section.",
                        "OK");

                    await GoToAsync($"//{RouteDashboard}");
                    return;
                }
            }

            if (_sessionService.IsAuthenticated && IsAuthRoute(target))
            {
                if (!IsRoute(target, RouteDashboard))
                {
                    e.Cancel();
                    await GoToAsync($"//{RouteDashboard}");
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}