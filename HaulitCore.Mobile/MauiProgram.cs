using HaulitCore.Mobile.DependencyInjection;
using HaulitCore.Mobile.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HaulitCore.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "HaulitCore-crashlogs.db");

        builder.Services.AddDbContextFactory<MobileCrashDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        builder.Services
            .AddMobileApplicationServices()
            .AddMobileHttpClients()
            .AddMobileViewModels()
            .AddMobilePages();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        using var scope = app.Services.CreateScope();
        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<MobileCrashDbContext>>();
        using var db = factory.CreateDbContext();
        db.Database.EnsureCreated();

        return app;
    }
}