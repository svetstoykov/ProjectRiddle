using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProjectRiddle.Infrastructure.Persistence;
using ProjectRiddle.IntegrationTests.Harness;

namespace ProjectRiddle.IntegrationTests;

/// <summary>
/// Verifies host startup, migrations, and the walking-skeleton health endpoint.
/// </summary>
public sealed class StartupMigrationTests
{
    /// <summary>
    /// Verifies that a fresh disposable SQLite database is migrated before a request is served.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task FreshDatabaseAppliesMigrationsBeforeServingRequests()
    {
        await using var workspace = TestWorkspace.Create();
        using var client = workspace.Factory.CreateClient();

        var response = await client.GetAsync("/api/system/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = workspace.Factory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ProjectRiddleDbContext>();
        var appliedMigrations = await database.Database.GetAppliedMigrationsAsync();

        Assert.Contains(appliedMigrations, name => name.EndsWith("_InitialBaseline", StringComparison.Ordinal));
        Assert.Contains(appliedMigrations, name => name.EndsWith("_AddUsersAndRiddles", StringComparison.Ordinal));
        Assert.True(await database.Database.CanConnectAsync());
    }

    /// <summary>
    /// Verifies that a migration failure aborts host creation instead of serving traffic.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task FailedMigrationPreventsTheHostFromServing()
    {
        var directory = Directory.CreateTempSubdirectory("project-riddle-");

        try
        {
            using var factory = new ApplicationFactory(directory.FullName, TestWorkspace.TimeZoneId);

            var exception = await Record.ExceptionAsync(async () =>
            {
                using var client = factory.CreateClient();
                _ = await client.GetAsync("/api/system/health");
            });

            Assert.NotNull(exception);
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }
}
