using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Views;
using Microsoft.Maui.ApplicationModel;
using System;
using System.Diagnostics;
using System.Linq;

namespace Haulory.Mobile;

// Central navigation shell for the app.
// Responsible for:
// 1) Registering routes (pages)
// 2) Enforcing auth rules:
//    - If not authenticated => force LoginPage for protected routes
//    - If authenticated => block Login/Register and send to Dashboard
public partial class AppShell : Shell
{
    #region Route Names (Single Source of Truth)

    public const string RouteDashboard = "DashboardPage";
    public const string RouteLogin = "LoginPage";
    public const string RouteRegister = "RegisterPage";

    #endregion

    #region Dependencies

    private readonly ISessionService _sessionService;

    #endregion

    #region Global Toolbar State

    private ToolbarItem? _homeToolbarItem;
    private bool _homeToolbarInitialized;

    #endregion

    #region Constructor

    public AppShell(ISessionService sessionService)
    {
        InitializeComponent();

        _sessionService = sessionService;

        RegisterRoutes();
        CreateHomeToolbarItem();

        Navigating += OnNavigating;
        Navigated += OnNavigated;
    }

    #endregion

    #region Public Navigation Helpers

    public Task GoHomeAsync() => GoToAsync($"//{RouteDashboard}");

    #endregion

    #region Global Toolbar

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
            var current = e.Current?.Location.OriginalString ?? string.Empty;

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

    #endregion

    #region Route Registration

    private static void RegisterRoutes()
    {
        // Jobs
        Routing.RegisterRoute(nameof(NewJobPage), typeof(NewJobPage));
        Routing.RegisterRoute(nameof(JobsCollectionPage), typeof(JobsCollectionPage));
        Routing.RegisterRoute(nameof(DeliverySignaturePage), typeof(DeliverySignaturePage));

        // Vehicles
        Routing.RegisterRoute(nameof(NewVehiclePage), typeof(NewVehiclePage));
        Routing.RegisterRoute(nameof(VehicleCollectionPage), typeof(VehicleCollectionPage));

        // Reports
        Routing.RegisterRoute(nameof(ReportsPage), typeof(ReportsPage));

        // Drivers
        Routing.RegisterRoute(nameof(DriverCollectionPage), typeof(DriverCollectionPage));
        Routing.RegisterRoute(nameof(NewDriverPage), typeof(NewDriverPage));
        Routing.RegisterRoute(nameof(EditDriverPage), typeof(EditDriverPage));

        // Inductions
        Routing.RegisterRoute(nameof(ManageInductionsPage), typeof(ManageInductionsPage));
        Routing.RegisterRoute(nameof(InductionTemplatesPage), typeof(InductionTemplatesPage));
        Routing.RegisterRoute(nameof(AddWorkSiteTemplatePage), typeof(AddWorkSiteTemplatePage));
    }

    #endregion

    #region Navigation Guard

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

    private async void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        var target = e.Target.Location.OriginalString;

        // Not authenticated -> block protected routes
        if (!_sessionService.IsAuthenticated && IsProtectedRoute(target))
        {
            if (!IsRoute(target, RouteLogin))
            {
                e.Cancel();
                await GoToAsync($"//{RouteLogin}");
            }
            return;
        }

        // Authenticated -> block auth routes
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

    #endregion
}