using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace HaulitCore.Infrastructure.Persistence;

public sealed class HaulitCoreDbContextFactory : IDesignTimeDbContextFactory<HaulitCoreDbContext>
{
    public HaulitCoreDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        var developmentApiPath = Path.Combine(basePath, "..", "Development.Api");
        var haulitCoreApiPath = Path.Combine(basePath, "..", "HaulitCore.Api");

        var settingsPath = Directory.Exists(developmentApiPath)
            ? developmentApiPath
            : Directory.Exists(haulitCoreApiPath)
                ? haulitCoreApiPath
                : basePath;

        var configuration = new ConfigurationBuilder()
            .SetBasePath(settingsPath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<HaulitCoreDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new HaulitCoreDbContext(optionsBuilder.Options);
    }
}