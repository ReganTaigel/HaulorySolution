using Haulory.Application.Features.Jobs;
using Haulory.Application.Features.Users;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Infrastructure.Persistence.Json;
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
            // JOBS – Application
            builder.Services.AddTransient<CreateJobHandler>();
            builder.Services.AddTransient<RegisterUserHandler>();
            builder.Services.AddTransient<LoginUserHandler>();
            builder.Services.AddSingleton<AppShell>();

            // JOBS – Repository
            builder.Services.AddSingleton<IJobRepository, JobRepository>();
            builder.Services.AddSingleton<IUserRepository, UserRepository>();
            builder.Services.AddSingleton<ISessionService, SessionService>();
            builder.Services.AddSingleton<IDeliveryReceiptRepository, DeliveryReceiptRepository>();
            builder.Services.AddSingleton<IVehicleAssetRepository, VehicleAssetRepository>();

            // JOBS – ViewModels
            builder.Services.AddTransient<NewJobViewModel>();
            builder.Services.AddTransient<JobsCollectionViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();
            builder.Services.AddTransient<DeliverySignatureViewModel>();
            builder.Services.AddTransient<ReportsViewModel>();
            builder.Services.AddTransient<NewVehicleViewModel>();
            builder.Services.AddTransient<VehicleCollectionViewModel>();

            // JOBS – Pages
            builder.Services.AddTransient<JobsCollectionPage>();
            builder.Services.AddTransient<NewJobPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();
            builder.Services.AddTransient<DeliverySignaturePage>();
            builder.Services.AddTransient<ReportsPage>();
            builder.Services.AddTransient<NewVehiclePage>();
            builder.Services.AddTransient<VehicleCollectionPage>();
#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
