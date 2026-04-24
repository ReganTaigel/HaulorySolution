using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Mobile.Diagnostics;

namespace HaulitCore.Mobile;

public partial class App : Microsoft.Maui.Controls.Application
{
    private readonly ISessionService _sessionService;
    private readonly AppShell _shell;
    private readonly ICrashLogger _crashLogger;
    private readonly CrashSyncService _crashSyncService;

    private bool _startupNavigationDone;
    private bool _resumeSyncRunning;

    public App(
        AppShell shell,
        ISessionService sessionService,
        ICrashLogger crashLogger,
        CrashSyncService crashSyncService)
    {
        InitializeComponent();

        _shell = shell;
        _sessionService = sessionService;
        _crashLogger = crashLogger;
        _crashSyncService = crashSyncService;

        RegisterGlobalExceptionHandlers();

        _shell.Loaded += OnShellLoaded;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(_shell);
    }

    private void RegisterGlobalExceptionHandlers()
    {
        AppDomain.CurrentDomain.UnhandledException += (_, e) =>
        {
            try
            {
                if (e.ExceptionObject is Exception ex)
                {
                    _crashLogger.TryLogCriticalImmediately(
                        ex,
                        "AppDomain.CurrentDomain.UnhandledException",
                        false,
                        "Critical",
                        "App");
                }
                else
                {
                    _crashLogger.TryLogMessageCriticalImmediately(
                        $"Unhandled non-exception object: {e.ExceptionObject}",
                        "AppDomain.CurrentDomain.UnhandledException",
                        false,
                        "Critical",
                        "App");
                }
            }
            catch
            {
            }
        };

        TaskScheduler.UnobservedTaskException += (_, e) =>
        {
            try
            {
                _crashLogger.TryLogCriticalImmediately(
                    e.Exception,
                    "TaskScheduler.UnobservedTaskException",
                    false,
                    "Critical",
                    "App");

                e.SetObserved();
            }
            catch
            {
            }
        };
    }

    private async void OnShellLoaded(object? sender, EventArgs e)
    {
        if (_startupNavigationDone)
            return;

        _startupNavigationDone = true;

        try
        {
            await _sessionService.RestoreAsync();

            // Run crash sync in the background so startup/navigation is not blocked.
            StartCrashSyncInBackground();

            if (_sessionService.IsAuthenticated)
            {
                await _shell.GoToAsync("///DashboardPage");
            }
            else
            {
                await _shell.GoToAsync("///LoginPage");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[App] Startup failure: {ex}");

            await _crashLogger.LogAsync(
                ex,
                "App.OnShellLoaded",
                true,
                "Error",
                "AppShell");

            try
            {
                await _shell.GoToAsync("///LoginPage");
            }
            catch (Exception navEx)
            {
                _crashLogger.TryLogCriticalImmediately(
                    navEx,
                    "App.OnShellLoaded.NavigationFallback",
                    true,
                    "Critical",
                    "AppShell");
            }
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        StartCrashSyncInBackground();
    }

    private void StartCrashSyncInBackground()
    {
        if (_resumeSyncRunning)
            return;

        _resumeSyncRunning = true;

        _ = Task.Run(async () =>
        {
            try
            {
                var pending = await _crashSyncService.GetPendingAsync();
                System.Diagnostics.Debug.WriteLine($"[App] Pending crash logs before sync: {pending.Count}");

                if (pending.Count == 0)
                    return;

                await _crashSyncService.SyncPendingAsync();
            }
            catch (Exception syncEx)
            {
                // Do not turn sync/network problems into more crash records during startup.
                // Just write to debug output for now.
                System.Diagnostics.Debug.WriteLine($"[App] Crash sync failed: {syncEx}");
            }
            finally
            {
                _resumeSyncRunning = false;
            }
        });
    }
}