using HaulitCore.Mobile.Views;

namespace HaulitCore.Mobile.DependencyInjection;

public static class PageRegistrationExtensions
{
    public static IServiceCollection AddMobilePages(this IServiceCollection services)
    {
        services.AddTransient<JobsCollectionPage>();
        services.AddTransient<NewJobPage>();
        services.AddTransient<DeliverySignaturePage>();
        services.AddTransient<RegisterPage>();
        services.AddTransient<LoginPage>();
        services.AddTransient<DashboardPage>();
        services.AddTransient<NeedsReviewPage>();
        services.AddTransient<ReportsPage>();
        services.AddTransient<NewVehiclePage>();
        services.AddTransient<VehicleCollectionPage>();
        services.AddTransient<DriverCollectionPage>();
        services.AddTransient<NewDriverPage>();
        services.AddTransient<ManageInductionsPage>();
        services.AddTransient<InductionTemplatesPage>();
        services.AddTransient<AddWorkSiteTemplatePage>();
        services.AddTransient<SettingsPage>();
        return services;
    }
}
