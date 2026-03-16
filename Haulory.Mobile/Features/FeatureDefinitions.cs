namespace Haulory.Mobile.Features;

public static class FeatureDefinitions
{
    public static readonly IReadOnlyDictionary<AppFeature, FeatureDefinition> Map =
        new Dictionary<AppFeature, FeatureDefinition>
        {
            [AppFeature.Dashboard] = new()
            {
                Feature = AppFeature.Dashboard
            },

            [AppFeature.StartDay] = new()
            {
                Feature = AppFeature.StartDay,
                Parent = AppFeature.Dashboard,
                DependsOn = new[]
                {
                    AppFeature.Drivers,
                    AppFeature.Vehicles
                }
            },

            [AppFeature.EndDay] = new()
            {
                Feature = AppFeature.EndDay,
                Parent = AppFeature.Dashboard,
                DependsOn = new[]
                {
                    AppFeature.Drivers,
                    AppFeature.Vehicles
                }
            },

            [AppFeature.QuickStats] = new()
            {
                Feature = AppFeature.QuickStats,
                Parent = AppFeature.Dashboard
            },

            [AppFeature.Jobs] = new()
            {
                Feature = AppFeature.Jobs,
                Parent = AppFeature.Dashboard
            },

            [AppFeature.AddJob] = new()
            {
                Feature = AppFeature.AddJob,
                Parent = AppFeature.Jobs
            },

            [AppFeature.JobBilling] = new()
            {
                Feature = AppFeature.JobBilling,
                Parent = AppFeature.Jobs
            },

            [AppFeature.JobPickup] = new()
            {
                Feature = AppFeature.JobPickup,
                Parent = AppFeature.Jobs
            },

            [AppFeature.JobDelivery] = new()
            {
                Feature = AppFeature.JobDelivery,
                Parent = AppFeature.Jobs
            },

            [AppFeature.JobLoadDetails] = new()
            {
                Feature = AppFeature.JobLoadDetails,
                Parent = AppFeature.Jobs
            },

            [AppFeature.JobAssignment] = new()
            {
                Feature = AppFeature.JobAssignment,
                Parent = AppFeature.Jobs,
                DependsOn = new[]
                {
                    AppFeature.Drivers,
                    AppFeature.Vehicles
                }
            },

            [AppFeature.DeliverySignature] = new()
            {
                Feature = AppFeature.DeliverySignature,
                Parent = AppFeature.Jobs,
                DependsOn = new[]
                {
                    AppFeature.JobDelivery
                }
            },

            [AppFeature.Vehicles] = new()
            {
                Feature = AppFeature.Vehicles,
                Parent = AppFeature.Dashboard
            },

            [AppFeature.AddVehicle] = new()
            {
                Feature = AppFeature.AddVehicle,
                Parent = AppFeature.Vehicles
            },

            [AppFeature.Drivers] = new()
            {
                Feature = AppFeature.Drivers,
                Parent = AppFeature.Dashboard
            },

            [AppFeature.AddDriver] = new()
            {
                Feature = AppFeature.AddDriver,
                Parent = AppFeature.Drivers
            },

            [AppFeature.Users] = new()
            {
                Feature = AppFeature.Users,
                Parent = AppFeature.Drivers
            },

            [AppFeature.Inductions] = new()
            {
                Feature = AppFeature.Inductions,
                Parent = AppFeature.Drivers
            },

            [AppFeature.Reports] = new()
            {
                Feature = AppFeature.Reports,
                Parent = AppFeature.Dashboard
            },

            [AppFeature.ExportPod] = new()
            {
                Feature = AppFeature.ExportPod,
                Parent = AppFeature.Reports,
                DependsOn = new[]
                {
                    AppFeature.Jobs,
                    AppFeature.JobDelivery,
                    AppFeature.DeliverySignature
                }
            },

            [AppFeature.ExportInvoice] = new()
            {
                Feature = AppFeature.ExportInvoice,
                Parent = AppFeature.Reports,
                DependsOn = new[]
                {
                    AppFeature.Jobs,
                    AppFeature.JobBilling
                }
            }
        };
}