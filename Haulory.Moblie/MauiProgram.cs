using Haulory.Application.Features.Drivers;
using Haulory.Application.Features.Jobs;
using Haulory.Application.Features.Users;
using Haulory.Application.Features.Vehicles.CreateVehicleSet;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Infrastructure.Persistence.Json;
using Haulory.Infrastructure.Repositories;
using Haulory.Infrastructure.Services;
using Haulory.Mobile.ViewModels;
using Haulory.Mobile.Views;
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

            // Repository
            builder.Services.AddSingleton<IJobRepository, JobRepository>();
            builder.Services.AddSingleton<IUserRepository, Haulory.Infrastructure.Persistence.Json.UserRepository>();
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddSingleton<IDeliveryReceiptRepository, DeliveryReceiptRepository>();
            builder.Services.AddSingleton<IVehicleAssetRepository, VehicleAssetRepository>();
            builder.Services.AddSingleton<IDriverRepository, DriverRepository>();

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
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
