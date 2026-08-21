using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectRiddle.Api.Models.System;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.IntegrationTests;

/// <summary>
/// Verifies the real host startup, migration, and shared HTTP error boundaries.
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
        var directory = Directory.CreateTempSubdirectory("project-riddle-phase-0-");
        var databasePath = Path.Combine(directory.FullName, "project-riddle.db");

        try
        {
            using var factory = new ApplicationFactory(databasePath, "Europe/Sofia");
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/system/ping");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var body = await response.Content.ReadFromJsonAsync<WalkingSkeletonResponse>();
            Assert.NotNull(body);
            Assert.Equal("Project Riddle is ready.", body.Message);
            Assert.Equal(new DateOnly(2026, 8, 21), body.PublicationDate);

            await using var scope = factory.Services.CreateAsyncScope();
            var database = scope.ServiceProvider.GetRequiredService<ProjectRiddleDbContext>();
            var appliedMigrations = await database.Database.GetAppliedMigrationsAsync();

            Assert.Contains("20260821000000_InitialBaseline", appliedMigrations);
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    /// <summary>
    /// Verifies that a Core Result failure is returned as Problem Details with its stable code.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task ResultFailureIsReturnedAsProblemDetails()
    {
        var directory = Directory.CreateTempSubdirectory("project-riddle-phase-0-");
        var databasePath = Path.Combine(directory.FullName, "project-riddle.db");

        try
        {
            using var factory = new ApplicationFactory(databasePath, "Europe/Sofia");
            using var client = factory.CreateClient();

            var response = await client.GetAsync("/api/system/ping?fail=true");
            var responseBody = await response.Content.ReadAsStringAsync();

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);

            using var document = JsonDocument.Parse(responseBody);
            Assert.Equal(
                "WalkingSkeleton.Failure",
                document.RootElement.GetProperty("code").GetString());
            Assert.Equal(
                "Validation failed",
                document.RootElement.GetProperty("title").GetString());
            Assert.True(document.RootElement.TryGetProperty("traceId", out _));
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    /// <summary>
    /// Verifies that a migration failure aborts host creation instead of serving traffic.
    /// </summary>
    /// <returns>A task that represents the test operation.</returns>
    [Fact]
    public async Task FailedMigrationPreventsTheHostFromServing()
    {
        var directory = Directory.CreateTempSubdirectory("project-riddle-phase-0-");

        try
        {
            using var factory = new ApplicationFactory(directory.FullName, "Europe/Sofia");

            var exception = await Record.ExceptionAsync(async () =>
            {
                using var client = factory.CreateClient();
                _ = await client.GetAsync("/api/system/ping");
            });

            Assert.NotNull(exception);
        }
        finally
        {
            Directory.Delete(directory.FullName, recursive: true);
        }
    }

    private sealed class ApplicationFactory : WebApplicationFactory<Program>
    {
        private readonly string databasePath;
        private readonly string timeZoneId;

        public ApplicationFactory(string databasePath, string timeZoneId)
        {
            this.databasePath = databasePath;
            this.timeZoneId = timeZoneId;
        }

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration(
                (_, configuration) =>
                {
                    configuration.AddInMemoryCollection(
                        new Dictionary<string, string?>
                        {
                            ["Persistence:DatabasePath"] = databasePath,
                            ["Publication:TimeZoneId"] = timeZoneId
                        });
                });
            builder.ConfigureTestServices(
                services =>
                {
                    services.RemoveAll<IClock>();
                    services.AddSingleton<IClock>(
                        new FixedClock(new DateTimeOffset(2026, 8, 20, 22, 30, 0, TimeSpan.Zero)));
                });
        }
    }

    private sealed class FixedClock : IClock
    {
        public FixedClock(DateTimeOffset utcNow)
        {
            UtcNow = utcNow;
        }

        public DateTimeOffset UtcNow { get; }
    }
}
