using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Infrastructure.Bootstrap;
using ProjectRiddle.Infrastructure.Configuration;
using ProjectRiddle.Infrastructure.Persistence;
using ProjectRiddle.Infrastructure.Repositories.Riddles;
using ProjectRiddle.Infrastructure.Time;

namespace ProjectRiddle.Infrastructure.Composition;

/// <summary>
/// Registers Infrastructure capabilities at the application composition boundary.
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    /// <summary>
    /// Adds SQLite persistence, validated options, and date-time services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <param name="configuration">The application configuration.</param>
    /// <returns>The supplied service collection.</returns>
    public static IServiceCollection AddProjectRiddleInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        services
            .AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services
            .AddOptions<TimeOptions>()
            .Bind(configuration.GetSection(TimeOptions.SectionName))
            .ValidateDataAnnotations()
            .Validate(
                options => CanFindTimeZone(options.TimeZoneId),
                "The configured application time zone must exist on the host.")
            .ValidateOnStart();

        services
            .AddOptions<AdminBootstrapOptions>()
            .Bind(configuration.GetSection(AdminBootstrapOptions.SectionName));

        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();
        services.AddScoped<IRiddleRepository, RiddleRepository>();
        services.AddHostedService<AdminBootstrapHostedService>();
        services.AddDbContext<ProjectRiddleDbContext>((serviceProvider, optionsBuilder) =>
        {
            var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
            var databasePath = Path.GetFullPath(databaseOptions.DatabasePath, hostEnvironment.ContentRootPath);
            var databaseDirectory = Path.GetDirectoryName(databasePath);

            if (databaseDirectory is not null)
            {
                Directory.CreateDirectory(databaseDirectory);
            }

            var connectionString = new SqliteConnectionStringBuilder
            {
                DataSource = databasePath
            }.ToString();

            optionsBuilder.UseSqlite(
                connectionString,
                sqliteOptions => sqliteOptions.MigrationsAssembly(typeof(ProjectRiddleDbContext).Assembly.FullName));
        });

        return services;
    }

    private static bool CanFindTimeZone(string? timeZoneId)
    {
        if (string.IsNullOrWhiteSpace(timeZoneId))
        {
            return false;
        }

        try
        {
            _ = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);
            return true;
        }
        catch (TimeZoneNotFoundException)
        {
            return false;
        }
        catch (InvalidTimeZoneException)
        {
            return false;
        }
    }
}
