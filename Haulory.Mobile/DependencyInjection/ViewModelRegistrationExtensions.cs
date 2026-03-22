using Haulory.Mobile.ViewModels;

namespace Haulory.Mobile.DependencyInjection;

public static class ViewModelRegistrationExtensions
{
    public static IServiceCollection AddMobileViewModels(this IServiceCollection services)
    {
        services.AddTransient<NewJobViewModel>();
        services.AddTransient<JobsCollectionViewModel>();
        services.AddTransient<DeliverySignatureViewModel>();
        services.AddTransient<RegisterViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<NeedsReviewViewModel>();
        services.AddTransient<ReportsViewModel>();
        services.AddTransient<NewVehicleViewModel>();
        services.AddTransient<VehicleCollectionViewModel>();
        services.AddTransient<DriverCollectionViewModel>();
        services.AddTransient<NewDriverViewModel>();
        services.AddTransient<ManageInductionsViewModel>();
        services.AddTransient<InductionTemplatesViewModel>();
        services.AddTransient<AddWorkSiteTemplateViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<AppShellViewModel>();
        return services;
    }
}
