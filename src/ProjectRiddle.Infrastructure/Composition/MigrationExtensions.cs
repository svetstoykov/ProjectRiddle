using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Infrastructure.Composition;

/// <summary>
/// Provides the startup migration boundary for the application host.
/// </summary>
public static partial class MigrationExtensions
{
    /// <summary>
    /// Applies all committed EF Core migrations before the host serves traffic.
    /// </summary>
    /// <param name="host">The application host.</param>
    /// <param name="cancellationToken">The token used to cancel migration execution.</param>
    /// <returns>A task that represents the migration operation.</returns>
    /// <exception cref="Exception">Propagates a migration failure so the host cannot start against an unknown schema.</exception>
    public static async Task ApplyProjectRiddleMigrationsAsync(
        this IHost host,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(host);

        using var scope = host.Services.CreateScope();
        var database = scope.ServiceProvider.GetRequiredService<ProjectRiddleDbContext>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<ProjectRiddleDbContext>>();

        try
        {
            await database.Database.MigrateAsync(cancellationToken);
            LogMigrationsApplied(logger);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            LogMigrationFailed(logger, exception);
            throw;
        }
    }

    [LoggerMessage(
        EventId = 1000,
        Level = LogLevel.Information,
        Message = "Project Riddle database migrations applied successfully.")]
    private static partial void LogMigrationsApplied(ILogger logger);

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Critical,
        Message = "Project Riddle database migration failed; application startup is aborting.")]
    private static partial void LogMigrationFailed(ILogger logger, Exception exception);
}
