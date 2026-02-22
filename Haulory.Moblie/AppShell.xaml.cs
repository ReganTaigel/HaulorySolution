using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Views;
using System;
using System.Linq;
using Microsoft.Maui.ApplicationModel;
using System.Diagnostics;

namespace Haulory.Mobile;

// Central navigation shell for the app.
// Responsible for:
// 1) Registering routes (pages)
// 2) Enforcing "bootstrap" rules:
//    - If no main user exists yet => force RegisterPage
//    - If user exists but not authenticated => force LoginPage for protected routes
//    - If authenticated => block Login/Register and send to Dashboard
public partial class AppShell : Shell
{
    #region Route Names (Single Source of Truth)

    // Keep these stable even if class names change later.
    public const string RouteDashboard = "DashboardPage";
    public const string RouteLogin = "LoginPage";
    public const string RouteRegister = "RegisterPage";

    #endregion

    #region Dependencies

    private readonly ISessionService _sessionService;
    private readonly IUserAccountRepository _userRepository;

    #endregion

    #region Cached State

    // Cached answer to "does a main user exist?".
    // Once it becomes true we never check again (fast path).
    private bool? _hasMainUser;

    #endregion

    #region Global Toolbar State

    // MAUI ToolbarItem does not support IsVisible, so we add/remove it dynamically.
    private ToolbarItem? _homeToolbarItem;
    private bool _homeToolbarInitialized;

    #endregion

    #region Constructor

    public AppShell(ISessionService sessionService, IUserAccountRepository userRepository)
    {
        InitializeComponent();

        _sessionService = sessionService;
        _userRepository = userRepository;

        RegisterRoutes();

        // Create a single app-wide Home toolbar item.
        // This is the global fix: Home always goes to Dashboard root (not back-stack).
        CreateHomeToolbarItem();

        // Navigation guard for auth + bootstrap rules
        Navigating += OnNavigating;

        // Keep toolbar state consistent across the whole app
        Navigated += OnNavigated;
    }

    #endregion

    #region Public Navigation Helpers

    // Always go to Dashboard root (resets Shell navigation stack)
    // Use this for your Top Bar "Home" button everywhere.
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
            // IconImageSource = "home.png"
        };

        _homeToolbarItem.Clicked += OnHomeToolbarClicked;

        _homeToolbarInitialized = true;
    }

    // Show/hide the Home button depending on where we are.
    // Hides Home on Login/Register to keep auth screens clean.
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

            // Always update UI collections on the main thread (Android can throw if not).
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
    private async void OnHomeClicked(object? sender, EventArgs e)
    {
        await GoHomeAsync();
    }
    #region Route Registration

    // Registers all pages with Shell routing.
    // Keep this grouped by feature for maintainability.
    private static void RegisterRoutes()
    {
        // NOTE:
        // If Login/Register/Dashboard are defined in AppShell.xaml via:
        // <ShellContent Route="DashboardPage" ... />
        // then DO NOT register them again here.
        //
        // If they are NOT defined with Route="..." in XAML,
        // uncomment these lines (and ensure the types exist).
        //
        // Routing.RegisterRoute(RouteLogin, typeof(LoginPage));
        // Routing.RegisterRoute(RouteRegister, typeof(RegisterPage));
        // Routing.RegisterRoute(RouteDashboard, typeof(DashboardPage));

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
        Routing.RegisterRoute(nameof(AddWorkSitePage), typeof(AddWorkSitePage));
        Routing.RegisterRoute(nameof(AddInductionRequirementPage), typeof(AddInductionRequirementPage));
    }

    #endregion

    #region Bootstrap Checks

    // Determines whether the app has at least one "main user" account created.
    // Caches once true to avoid hitting storage repeatedly on every navigation.
    private async Task<bool> HasMainUserAsync()
    {
        // Once true, cache it forever (fast path)
        if (_hasMainUser == true)
            return true;

        // Otherwise keep checking until it becomes true
        _hasMainUser = await _userRepository.AnyAsync();
        return _hasMainUser.Value;
    }

    #endregion

    #region Navigation Guard

    private static bool IsRoute(string target, string route) =>
        target.Contains(route, StringComparison.OrdinalIgnoreCase);

    // Routes that should require an authenticated session.
    // Keep these centralized so you don't scatter string checks around the app.
    private static readonly string[] ProtectedRoutes =
    {
        RouteDashboard,
        nameof(DriverCollectionPage),
        nameof(VehicleCollectionPage),
        nameof(JobsCollectionPage),
        nameof(ReportsPage),

        // Inductions should also be protected
        nameof(ManageInductionsPage),
        nameof(InductionTemplatesPage),
        nameof(AddWorkSitePage),
        nameof(AddInductionRequirementPage),
    };

    // Routes that should be inaccessible once authenticated.
    private static readonly string[] AuthRoutes =
    {
        RouteLogin,
        RouteRegister
    };

    private static bool IsProtectedRoute(string target) =>
        ProtectedRoutes.Any(r => target.Contains(r, StringComparison.OrdinalIgnoreCase));

    private static bool IsAuthRoute(string target) =>
        AuthRoutes.Any(r => target.Contains(r, StringComparison.OrdinalIgnoreCase));

    // Navigation gatekeeper:
    // - Restores session on cold start
    // - Forces registration until a main user exists
    // - Forces login for protected pages
    // - Prevents showing login/register once authenticated
    private async void OnNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        // Always restore session on cold start before applying routing rules.
        if (!_sessionService.IsAuthenticated)
            await _sessionService.RestoreAsync();

        var target = e.Target.Location.OriginalString;

        // BOOTSTRAP: if no main user exists yet, force RegisterPage
        var hasMainUser = await HasMainUserAsync();
        if (!hasMainUser)
        {
            // Avoid re-entrancy loops if we're already going there
            if (!IsRoute(target, RouteRegister))
            {
                e.Cancel();
                await GoToAsync($"//{RouteRegister}");
            }
            return;
        }

        // If user exists but not authenticated, block protected app pages
        if (!_sessionService.IsAuthenticated && IsProtectedRoute(target))
        {
            if (!IsRoute(target, RouteLogin))
            {
                e.Cancel();
                await GoToAsync($"//{RouteLogin}");
            }
            return;
        }

        // If authenticated, block Login/Register pages
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