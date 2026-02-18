using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Mobile.Views;

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
    #region Dependencies

    private readonly ISessionService _sessionService;
    private readonly IUserAccountRepository _userRepository;

    #endregion

    #region Cached State

    // Cached answer to "does a main user exist?".
    // Once it becomes true we never check again (fast path).
    private bool? _hasMainUser;

    #endregion

    #region Constructor

    public AppShell(ISessionService sessionService, IUserAccountRepository userRepository)
    {
        InitializeComponent();

        _sessionService = sessionService;
        _userRepository = userRepository;

        RegisterRoutes();

        // Navigation guard for auth + bootstrap rules
        Navigating += OnNavigating;
    }

    #endregion

    #region Route Registration

    // Registers all pages with Shell routing.
    // Keep this grouped by feature for maintainability.
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
            if (!target.Contains("RegisterPage"))
            {
                e.Cancel();
                await GoToAsync("///RegisterPage");
            }
            return;
        }

        // If user exists but not authenticated, block protected app pages
        if (!_sessionService.IsAuthenticated && IsProtectedRoute(target))
        {
            e.Cancel();
            await GoToAsync("///LoginPage");
            return;
        }

        // If authenticated, block Login/Register pages
        if (_sessionService.IsAuthenticated && IsAuthRoute(target))
        {
            e.Cancel();
            await GoToAsync("///DashboardPage");
            return;
        }
    }

    // Routes that should require an authenticated session.
    // NOTE: Prefer centralizing route names here instead of scattering string checks.
    private static bool IsProtectedRoute(string target) =>
        target.Contains("DashboardPage") ||
        target.Contains("DriverCollectionPage") ||
        target.Contains("VehicleCollectionPage") ||
        target.Contains("JobsCollectionPage") ||
        target.Contains("ReportsPage");

    // Routes that should be inaccessible once authenticated.
    private static bool IsAuthRoute(string target) =>
        target.Contains("LoginPage") ||
        target.Contains("RegisterPage");

    #endregion
}
