using Haulory.Application.Features.Drivers;
using Haulory.Application.Features.Jobs;
using Haulory.Application.Features.Reports;
using Haulory.Application.Features.Users;
using Haulory.Application.Features.Vehicles.CreateVehicleSet;
using Haulory.Application.Interfaces.Repositories;
using Haulory.Application.Interfaces.Services;
using Haulory.Application.Services;
using Haulory.Infrastructure.Persistence.Repositories;
using Haulory.Infrastructure.Persistence.Services;
using Haulory.Infrastructure.Services;
using Haulory.Mobile.Diagnostics;
using Haulory.Mobile.Features;
using Haulory.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Haulory.Mobile.DependencyInjection;

public static class ApplicationServiceRegistrationExtensions
{
    public static IServiceCollection AddMobileApplicationServices(this IServiceCollection services)
    {
        services.AddSingleton<IFeatureAccessService, FeatureAccessService>();
        services.AddSingleton<AppShell>();

        services.AddTransient<CreateJobHandler>();
        services.AddTransient<CreateSubUserHandler>();
        services.AddTransient<CreateDriverFromUserHandler>();
        services.AddTransient<CreateDriverHandler>();
        services.AddTransient<CreateVehicleHandler>();
        services.AddTransient<InvoiceReportHandler>();
        services.AddTransient<PodReportHandler>();

        services.AddSingleton<OdometerApiService>();
        services.AddSingleton<ISessionService, SessionService>();
        services.AddSingleton<ICrashLogger, CrashLogger>();
        services.AddSingleton<CrashSyncService>();

        services.AddTransient<IPdfInvoiceGenerator, PdfInvoiceGenerator>();
        services.AddTransient<IPdfPodGenerator, PdfPodGenerator>();
        services.AddScoped<IVehicleDayRunRepository, VehicleDayRunRepository>();
        services.AddScoped<IComplianceEnsurer, ComplianceEnsurer>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IInvoiceCalculationService, InvoiceCalculationService>();

        services.AddSingleton<AuthApiService>();
        services.AddSingleton<DriversApiService>();
        services.AddSingleton<JobsApiService>();
        services.AddSingleton<VehiclesApiService>();
        services.AddSingleton<ReportsApiService>();
        services.AddTransient<CustomersApiService>();
        services.AddSingleton<DocumentSettingsApiService>();
        return services;
    }
}