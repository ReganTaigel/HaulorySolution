using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Features;
using Haulory.Mobile.Views;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Diagnostics;
using System.Linq;

namespace Haulory.Mobile;

public partial class AppShell : Shell
{
    public const string RouteDashboard = "DashboardPage";
    public const string RouteLogin = "LoginPage";
    public const string RouteRegister = "RegisterPage";

    private readonly ISessionService _sessionService;
    private readonly IFeatureAccessService _featureAccessService;

    private ToolbarItem? _homeToolbarItem;
    private bool _homeToolbarInitialized;

    public AppShell(
        ISessionService sessionService,
        IFeatureAccessService featureAccessService)
    {
        InitializeComponent();

        _sessionService = sessionService;
        _featureAccessService = featureAccessService;

        RegisterRoutes();
        CreateHomeToolbarItem();

        Navigating += OnNavigating;
        Navigated += OnNavigated;
    }

    public Task GoHomeAsync() => GoToAsync($"//{RouteDashboard}");

    private async void OnHomeToolbarClicked(object? sender, EventArgs e)
    {
        try
        {
            await GoHomeAsync();
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private void CreateHomeToolbarItem()
    {
        if (_homeToolbarInitialized)
            return;

        _homeToolbarItem = new ToolbarItem
        {
            Text = "Home",
            Priority = 0,
            Order = ToolbarItemOrder.Primary
        };

        _homeToolbarItem.Clicked += OnHomeToolbarClicked;
        _homeToolbarInitialized = true;
    }

    private void OnNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        try
        {
            var current = e.Current?.Location?.OriginalString ?? string.Empty;

            var isAuthScreen =
                current.Contains(RouteLogin, StringComparison.OrdinalIgnoreCase) ||
                current.Contains(RouteRegister, StringComparison.OrdinalIgnoreCase);

            var shouldShowHome = _sessionService.IsAuthenticated && !isAuthScreen;

            if (_homeToolbarItem == null)
                return;

            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    if (shouldShowHome)
                    {
                        if (!ToolbarItems.Contains(_homeToolbarItem))
                            ToolbarItems.Add(_homeToolbarItem);

                        _homeToolbarItem.IsEnabled = true;
                    }
                    else
                    {
                        if (ToolbarItems.Contains(_homeToolbarItem))
                            ToolbarItems.Remove(_homeToolbarItem);
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }

    private static void RegisterRoutes()
    {
        Routing.RegisterRoute(nameof(NewJobPage), typeof(NewJobPage));
        Routing.RegisterRoute(nameof(JobsCollectionPage), typeof(JobsCollectionPage));
        Routing.RegisterRoute(nameof(DeliverySignaturePage), typeof(DeliverySignaturePage));

        Routing.RegisterRoute(nameof(NewVehiclePage), typeof(NewVehiclePage));
        Routing.RegisterRoute(nameof(VehicleCollectionPage), typeof(VehicleCollectionPage));

        Routing.RegisterRoute(nameof(ReportsPage), typeof(ReportsPage));

        Routing.RegisterRoute(nameof(DriverCollectionPage), typeof(DriverCollectionPage));
        Routing.RegisterRoute(nameof(NewDriverPage), typeof(NewDriverPage));
        Routing.RegisterRoute(nameof(EditDriverPage), typeof(EditDriverPage));

        Routing.RegisterRoute(nameof(ManageInductionsPage), typeof(ManageInductionsPage));
        Routing.RegisterRoute(nameof(InductionTemplatesPage), typeof(InductionTemplatesPage));
        Routing.RegisterRoute(nameof(AddWorkSiteTemplatePage), typeof(AddWorkSiteTemplatePage));
    }

    private static bool IsRoute(string target, string route) =>
        target.Contains(route, StringComparison.OrdinalIgnoreCase);

    private static readonly string[] ProtectedRoutes =
    {
        RouteDashboard,
        nameof(DriverCollectionPage),
        nameof(VehicleCollectionPage),
        nameof(JobsCollectionPage),
        nameof(ReportsPage),
        nameof(ManageInductionsPage),
        nameof(InductionTemplatesPage),
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

        if (target.Contains(nameof(DriverCollectionPage), StringComparison.OrdinalIgnoreCase) ||
            target.Contains(nameof(EditDriverPage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Drivers);
        }

        if (target.Contains(nameof(NewDriverPage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.AddDriver);
        }

        if (target.Contains(nameof(JobsCollectionPage), StringComparison.OrdinalIgnoreCase) ||
            target.Contains(nameof(DeliverySignaturePage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Jobs);
        }

        if (target.Contains(nameof(NewJobPage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.AddJob);
        }

        if (target.Contains(nameof(VehicleCollectionPage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Vehicles);
        }

        if (target.Contains(nameof(NewVehiclePage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.AddVehicle);
        }

        if (target.Contains(nameof(ReportsPage), StringComparison.OrdinalIgnoreCase))
        {
            return _featureAccessService.GetAccess(AppFeature.Reports);
        }

        return null;
    }

    private async void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        try
        {
            if (!_sessionService.IsAuthenticated)
                await _sessionService.RestoreAsync();

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

            if (_sessionService.IsAuthenticated && IsAuthRoute(target))
            {
                if (!IsRoute(target, RouteDashboard))
                {
                    e.Cancel();
                    await GoToAsync($"//{RouteDashboard}");
                }
                return;
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine(ex);
        }
    }
}