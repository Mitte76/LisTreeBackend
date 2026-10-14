using System;
using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Design;

public class AppDbContextFactory : IDesignTimeDbContextFactory<Infrastructure.Data.AppDbContext>
{
    public Infrastructure.Data.AppDbContext CreateDbContext(string[] args)
    {
        // Try to locate API project's appsettings.json by walking up and into LisTreeBackend folder
        var basePath = Directory.GetCurrentDirectory();

        // If running from solution root or Infrastructure folder, try to find LisTreeBackend/appsettings.json
        var tryPaths = new[]
        {
            Path.Combine(basePath, "LisTreeBackend"),
            Path.Combine(basePath, "..", "LisTreeBackend"),
            Path.Combine(basePath, "..", "..", "LisTreeBackend"),
        };

        string? configDir = null;
        foreach (var p in tryPaths)
        {
            var configFile = Path.Combine(p, "appsettings.json");
            if (File.Exists(configFile))
            {
                configDir = p;
                break;
            }
        }

        var builder = new ConfigurationBuilder();
        if (configDir != null)
        {
            builder.SetBasePath(configDir).AddJsonFile("appsettings.json", optional: true);
        }
        else
        {
            // fallback to current directory
            builder.SetBasePath(basePath);
        }

        builder.AddEnvironmentVariables();
        var configuration = builder.Build();

        var connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrEmpty(connectionString))
        {
            // Fallback: read from environment variable or throw
            connectionString = Environment.GetEnvironmentVariable("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found. Set it in LisTreeBackend/appsettings.json or as environment variable.");
        }

        var optionsBuilder = new DbContextOptionsBuilder<Infrastructure.Data.AppDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new Infrastructure.Data.AppDbContext(optionsBuilder.Options);
    }
}
