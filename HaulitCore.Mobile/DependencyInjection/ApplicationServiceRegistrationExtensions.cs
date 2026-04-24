using HaulitCore.Application.Features.Drivers;
using HaulitCore.Application.Features.Jobs;
using HaulitCore.Application.Features.Reports;
using HaulitCore.Application.Features.Users;
using HaulitCore.Application.Features.Vehicles.CreateVehicleSet;
using HaulitCore.Application.Interfaces.Repositories;
using HaulitCore.Application.Interfaces.Services;
using HaulitCore.Application.Services;
using HaulitCore.Infrastructure.Persistence.Repositories;
using HaulitCore.Infrastructure.Persistence.Services;
using HaulitCore.Infrastructure.Services;
using HaulitCore.Mobile.Diagnostics;
using HaulitCore.Mobile.Features;
using HaulitCore.Mobile.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HaulitCore.Mobile.DependencyInjection;

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

        services.AddSingleton<HubodometerApiService>();
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
        services.AddSingleton<BusinessProfileApiService>();

        return services;
    }
}