using Haulory.Application.Features.Drivers;
using Haulory.Application.Features.Jobs;
using Haulory.Application.Features.Reports;
using Haulory.Application.Features.Users;
using Haulory.Application.Features.Vehicles.CreateVehicleSet;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Application.Services;
using Haulory.Infrastructure.Persistence;
using Haulory.Infrastructure.Persistence.Repositories;
using Haulory.Infrastructure.Persistence.Services;
using Haulory.Infrastructure.Services;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Haulory.Mobile.ViewModels;
using Haulory.Mobile.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Haulory.Mobile;

// MAUI application bootstrap:
// - Configures fonts and app services
// - Registers DI dependencies (handlers, repositories, services, VMs, pages)
// - Initializes local SQLite database
public static class MauiProgram
{
    #region Public API

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

        RegisterApplicationServices(builder);
        RegisterDatabase(builder);
        RegisterRepositories(builder);
        RegisterViewModels(builder);
        RegisterPages(builder);

#if DEBUG
        builder.Logging.AddDebug();
#endif

        var app = builder.Build();

        EnsureDatabaseCreated(app);

        return app;
    }

    #endregion

    #region Service Registration

    // Domain/application-level services and handlers.
    private static void RegisterApplicationServices(MauiAppBuilder builder)
    {
        builder.Services.AddSingleton<IFeatureAccessService, FeatureAccessService>();
        builder.Services.AddSingleton<AppShell>();

        builder.Services.AddTransient<CreateJobHandler>();
        builder.Services.AddTransient<CreateSubUserHandler>();
        builder.Services.AddTransient<CreateDriverFromUserHandler>();
        builder.Services.AddTransient<CreateDriverHandler>();
        builder.Services.AddTransient<CreateVehicleHandler>();

        builder.Services.AddTransient<InvoiceReportHandler>();
        builder.Services.AddTransient<PodReportHandler>();

        builder.Services.AddTransient<IPdfInvoiceGenerator, PdfInvoiceGenerator>();
        builder.Services.AddTransient<IPdfPodGenerator, PdfPodGenerator>();

        builder.Services.AddSingleton<ISessionService, SessionService>();

        builder.Services.AddScoped<IComplianceEnsurer, ComplianceEnsurer>();

        builder.Services.AddSingleton(new HttpClient
        {
            BaseAddress = new Uri("http://10.0.2.2:5158/")
        });

        // API services
        builder.Services.AddSingleton<AuthApiService>();
        builder.Services.AddSingleton<DriversApiService>();
        builder.Services.AddSingleton<JobsApiService>();
        builder.Services.AddSingleton<VehiclesApiService>();
        builder.Services.AddSingleton<ReportsApiService>();
    }

    #endregion

    #region Database

    // Registers EF Core DbContext factory using SQLite in app data directory.
    private static void RegisterDatabase(MauiAppBuilder builder)
    {
        var dbPath = Path.Combine(FileSystem.AppDataDirectory, "haulory.db");

        builder.Services.AddDbContextFactory<HauloryDbContext>(options =>
            options.UseSqlite($"Filename={dbPath}")
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
        );
    }

    // Ensures local database exists on app start.
    private static void EnsureDatabaseCreated(MauiApp app)
    {
        using var scope = app.Services.CreateScope();

        var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<HauloryDbContext>>();
        using var db = factory.CreateDbContext();

        db.Database.EnsureCreated();
    }

    #endregion

    #region Repositories

    // Data access registration.
    // IMPORTANT: repositories are scoped (not singleton) because they depend on DbContext.
    private static void RegisterRepositories(MauiAppBuilder builder)
    {
        builder.Services.AddScoped<IUserAccountRepository, UserAccountRepository>();
        builder.Services.AddScoped<IJobRepository, JobRepository>();
        builder.Services.AddScoped<IDriverRepository, DriverRepository>();
        builder.Services.AddScoped<IDeliveryReceiptRepository, DeliveryReceiptRepository>();
        builder.Services.AddScoped<IVehicleAssetRepository, VehicleAssetRepository>();
        builder.Services.AddScoped<IOdometerService, OdometerServiceRepository>();
        builder.Services.AddScoped<IDriverInductionRepository, DriverInductionRepository>();
        builder.Services.AddScoped<IWorkSiteRepository, WorkSiteRepository>();
        builder.Services.AddScoped<IInductionRequirementRepository, InductionRequirementRepository>();
    }

    #endregion

    #region ViewModels

    private static void RegisterViewModels(MauiAppBuilder builder)
    {
        // Jobs
        builder.Services.AddTransient<NewJobViewModel>();
        builder.Services.AddTransient<JobsCollectionViewModel>();
        builder.Services.AddTransient<DeliverySignatureViewModel>();

        // Register/Login User
        builder.Services.AddTransient<RegisterViewModel>();
        builder.Services.AddTransient<LoginViewModel>();

        // Dashboard
        builder.Services.AddTransient<DashboardViewModel>();

        // Reports
        builder.Services.AddTransient<ReportsViewModel>();

        // Vehicles
        builder.Services.AddTransient<NewVehicleViewModel>();
        builder.Services.AddTransient<VehicleCollectionViewModel>();

        // Drivers
        builder.Services.AddTransient<DriverCollectionViewModel>();
        builder.Services.AddTransient<NewDriverViewModel>();
        builder.Services.AddTransient<EditDriverViewModel>();

        // Inductions
        builder.Services.AddTransient<ManageInductionsViewModel>();
        builder.Services.AddTransient<InductionTemplatesViewModel>();
        builder.Services.AddTransient<AddWorkSiteTemplateViewModel>();
    }

    #endregion

    #region Pages

    private static void RegisterPages(MauiAppBuilder builder)
    {
        // Jobs
        builder.Services.AddTransient<JobsCollectionPage>();
        builder.Services.AddTransient<NewJobPage>();
        builder.Services.AddTransient<DeliverySignaturePage>();

        // Register/Login User
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<LoginPage>();

        // Dashboard
        builder.Services.AddTransient<DashboardPage>();

        // Reports
        builder.Services.AddTransient<ReportsPage>();

        // Vehicles
        builder.Services.AddTransient<NewVehiclePage>();
        builder.Services.AddTransient<VehicleCollectionPage>();

        // Drivers
        builder.Services.AddTransient<DriverCollectionPage>();
        builder.Services.AddTransient<NewDriverPage>();
        builder.Services.AddTransient<EditDriverPage>();

        // Inductions
        builder.Services.AddTransient<ManageInductionsPage>();
        builder.Services.AddTransient<InductionTemplatesPage>();
        builder.Services.AddTransient<AddWorkSiteTemplatePage>();
    }

    #endregion
}