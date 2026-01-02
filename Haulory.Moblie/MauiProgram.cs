using Haulory.Application.Features.Users;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Infrastructure.Persistence.InMemory;
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

            builder.Services.AddSingleton<AppShell>();

            builder.Services.AddSingleton<IUserRepository, UserRepository>();

            builder.Services.AddTransient<RegisterUserHandler>();
            builder.Services.AddTransient<LoginUserHandler>();
            builder.Services.AddTransient<RegisterViewModel>();
            builder.Services.AddTransient<LoginViewModel>();

            builder.Services.AddTransient<RegisterPage>();
            builder.Services.AddTransient<LoginPage>();


#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
