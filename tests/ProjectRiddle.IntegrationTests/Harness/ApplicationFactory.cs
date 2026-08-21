using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ProjectRiddle.Core.Interfaces.Time;

namespace ProjectRiddle.IntegrationTests.Harness;

/// <summary>
/// Hosts the application against a disposable SQLite database for integration tests.
/// </summary>
public sealed class ApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string databasePath;
    private readonly string timeZoneId;
    private readonly DateTimeOffset? utcNow;
    private readonly string bootstrapEmail;
    private readonly string bootstrapPassword;

    /// <summary>
    /// Initializes the test host factory.
    /// </summary>
    /// <param name="databasePath">The SQLite database path.</param>
    /// <param name="timeZoneId">The configured local time-zone identifier.</param>
    /// <param name="utcNow">The optional fixed UTC instant.</param>
    /// <param name="bootstrapEmail">The optional bootstrap administrator email.</param>
    /// <param name="bootstrapPassword">The optional bootstrap administrator password.</param>
    public ApplicationFactory(
        string databasePath,
        string timeZoneId,
        DateTimeOffset? utcNow = null,
        string? bootstrapEmail = null,
        string? bootstrapPassword = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        this.databasePath = databasePath;
        this.timeZoneId = timeZoneId;
        this.utcNow = utcNow;
        this.bootstrapEmail = bootstrapEmail ?? string.Empty;
        this.bootstrapPassword = bootstrapPassword ?? string.Empty;
    }

    /// <inheritdoc />
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration(
            (_, configuration) =>
            {
                configuration.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Persistence:DatabasePath"] = databasePath,
                        ["Time:TimeZoneId"] = timeZoneId,
                        ["AdminBootstrap:Email"] = bootstrapEmail,
                        ["AdminBootstrap:Password"] = bootstrapPassword,
                        ["Seq:ServerUrl"] = string.Empty
                    });
            });

        if (utcNow is null)
        {
            return;
        }

        var clock = new FixedDateTimeProvider(utcNow.Value, timeZoneId);
        builder.ConfigureTestServices(
            services =>
            {
                services.RemoveAll<IDateTimeProvider>();
                services.AddSingleton<IDateTimeProvider>(clock);
                services.AddSingleton(clock);
            });
    }
}
