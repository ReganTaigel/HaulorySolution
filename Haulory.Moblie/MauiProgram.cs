using Haulory.Application.Features.Jobs;
using Haulory.Application.Features.Users;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Infrastructure.Persistence.Json;
using Haulory.Infrastructure.Services;
using Haulory.Moblie.ViewModels;
using Haulory.Moblie.Views;
using Microsoft.Extensions.Logging;

namespace Haulory.Moblie
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

            // JOBS – ViewModels
            builder.Services.AddTransient<NewJobViewModel>();
            builder.Services.AddTransient<JobsCollectionViewModel>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<LoginViewModel>();
            builder.Services.AddTransient<DashboardViewModel>();

            // JOBS – Pages
            builder.Services.AddTransient<JobsCollectionPage>();
            builder.Services.AddTransient<NewJobPage>();
            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<DashboardPage>();













#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
