using Haulory.Application.Features.Drivers;
using Haulory.Application.Features.Jobs;
using Haulory.Application.Features.Users;
using Haulory.Application.Features.Vehicles.CreateVehicleSet;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Infrastructure.Persistence;
using Haulory.Infrastructure.Persistence.Repositories;
using Haulory.Infrastructure.Persistence.Services;
using Haulory.Mobile.ViewModels;
using Haulory.Mobile.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Haulory.Mobile
{
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

            // Application
            builder.Services.AddTransient<CreateJobHandler>();
            builder.Services.AddTransient<RegisterUserHandler>();
            builder.Services.AddTransient<LoginUserHandler>();
            builder.Services.AddSingleton<AppShell>();
            builder.Services.AddTransient<CreateDriverFromUserHandler>();
            builder.Services.AddTransient<CreateDriverHandler>();

            //DBContext

            var dbPath = Path.Combine(FileSystem.AppDataDirectory, "haulory.db");

            builder.Services.AddDbContextFactory<HauloryDbContext>(options =>
                options.UseSqlite($"Filename={dbPath}")
#if DEBUG
                .EnableSensitiveDataLogging()
#endif
            );
            // Repository
            // IMPORTANT: Scoped (not Singleton)
            builder.Services.AddScoped<IUserAccountRepository, Infrastructure.Persistence.Repositories.UserAccountRepository>();
            builder.Services.AddScoped<IJobRepository, Infrastructure.Persistence.Repositories.JobRepository>();
            builder.Services.AddScoped<IDriverRepository, Infrastructure.Persistence.Repositories.DriverRepository>();
            builder.Services.AddScoped<IDeliveryReceiptRepository, Infrastructure.Persistence.Repositories.DeliveryReceiptRepository>();
            builder.Services.AddScoped<IVehicleAssetRepository, Infrastructure.Persistence.Repositories.VehicleAssetRepository>();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            builder.Services.AddScoped<IComplianceEnsurer, ComplianceEnsurer>();
            builder.Services.AddScoped<IDriverInductionRepository, DriverInductionRepository>();
            builder.Services.AddScoped<IWorkSiteRepository, WorkSiteRepository>();
            builder.Services.AddScoped<IInductionRequirementRepository, InductionRequirementRepository>();


            // Session can remain Singleton (does not depend on DbContext directly)
            builder.Services.AddSingleton<ISessionService, SessionService>();

            // ViewModels
            // Jobs
            builder.Services.AddTransient<NewJobViewModel>();
            builder.Services.AddTransient<JobsCollectionViewModel>();
            builder.Services.AddTransient<DeliverySignatureViewModel>();

            // Register/Login User
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<LoginViewModel>();

            // Dashbord
            builder.Services.AddTransient<DashboardViewModel>();

            // Reports
            builder.Services.AddTransient<ReportsViewModel>();

            // Vehicle 
            builder.Services.AddTransient<NewVehicleViewModel>();
            builder.Services.AddTransient<VehicleCollectionViewModel>();

            // Driver
            builder.Services.AddTransient<DriverCollectionViewModel>();
            builder.Services.AddTransient<NewDriverViewModel>();
            builder.Services.AddTransient<EditDriverViewModel>();

            //Inductions
            builder.Services.AddTransient<AddWorkSiteViewModel>();
            builder.Services.AddTransient<AddInductionRequirementViewModel>();
            builder.Services.AddTransient<ManageInductionsViewModel>();
            builder.Services.AddTransient<InductionTemplatesViewModel>();

            // Pages
            //Jobs
            builder.Services.AddTransient<JobsCollectionPage>();
            builder.Services.AddTransient<NewJobPage>();
            builder.Services.AddTransient<DeliverySignaturePage>();

            // Register/Login User
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<LoginPage>();

            // Dashbord
            builder.Services.AddTransient<DashboardPage>();

            // Reports
            builder.Services.AddTransient<ReportsPage>();

            // Vehicle 
            builder.Services.AddTransient<NewVehiclePage>();
            builder.Services.AddTransient<VehicleCollectionPage>();
            builder.Services.AddTransient<CreateVehicleHandler>();

            // Drivers
            builder.Services.AddTransient<DriverCollectionPage>();
            builder.Services.AddTransient<NewDriverPage>();
            builder.Services.AddTransient<EditDriverPage>();

            //Inductions
            builder.Services.AddTransient<ManageInductionsPage>();
            builder.Services.AddTransient<AddWorkSitePage>();
            builder.Services.AddTransient<AddInductionRequirementPage>();
            builder.Services.AddTransient<InductionTemplatesPage>();

#if DEBUG
            builder.Logging.AddDebug();
#endif

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<HauloryDbContext>>();
                using var db = factory.CreateDbContext();
                db.Database.EnsureCreated();
            }

            return app;

        }
    }
}
