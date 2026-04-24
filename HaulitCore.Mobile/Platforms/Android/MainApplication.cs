using Android.App;
using Android.Runtime;
using HaulitCore.Mobile.Diagnostics;

namespace HaulitCore.Mobile;

[Application]
public class MainApplication : MauiApplication
{
    public MainApplication(IntPtr handle, JniHandleOwnership ownership)
        : base(handle, ownership)
    {
    }

    protected override MauiApp CreateMauiApp()
    {
        AndroidEnvironment.UnhandledExceptionRaiser += (_, e) =>
        {
            try
            {
                var services = IPlatformApplication.Current?.Services;
                var logger = services?.GetService<ICrashLogger>();

                logger?.TryLogCriticalImmediately(
                    e.Exception,
                    "AndroidEnvironment.UnhandledExceptionRaiser",
                    false,
                    "Critical",
                    "Android");
            }
            catch
            {
            }
        };

        return MauiProgram.CreateMauiApp();
    }
}