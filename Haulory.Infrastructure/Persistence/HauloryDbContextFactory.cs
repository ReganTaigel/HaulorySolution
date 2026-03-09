using Haulory.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Haulory.Infrastructure.Persistence;

public sealed class HauloryDbContextFactory : IDesignTimeDbContextFactory<HauloryDbContext>
{
    public HauloryDbContext CreateDbContext(string[] args)
    {
        var basePath = Directory.GetCurrentDirectory();

        // Try to locate the API project appsettings.json when running from PMC
        var apiPath = Path.Combine(basePath, "..", "Haulory.Api");

        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.Exists(apiPath) ? apiPath : basePath)
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        var optionsBuilder = new DbContextOptionsBuilder<HauloryDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        return new HauloryDbContext(optionsBuilder.Options);
    }
}