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
    private readonly string _databasePath;
    private readonly string _timeZoneId;
    private readonly DateTimeOffset? _utcNow;
    private readonly string _bootstrapEmail;
    private readonly string _bootstrapPassword;

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

        this._databasePath = databasePath;
        this._timeZoneId = timeZoneId;
        this._utcNow = utcNow;
        this._bootstrapEmail = bootstrapEmail ?? string.Empty;
        this._bootstrapPassword = bootstrapPassword ?? string.Empty;
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
                        ["Persistence:DatabasePath"] = _databasePath,
                        ["Time:TimeZoneId"] = _timeZoneId,
                        ["AdminBootstrap:Email"] = _bootstrapEmail,
                        ["AdminBootstrap:Password"] = _bootstrapPassword,
                        ["Seq:ServerUrl"] = string.Empty
                    });
            });

        if (_utcNow is null)
        {
            return;
        }

        var clock = new FixedDateTimeProvider(_utcNow.Value, _timeZoneId);
        builder.ConfigureTestServices(
            services =>
            {
                services.RemoveAll<IDateTimeProvider>();
                services.AddSingleton<IDateTimeProvider>(clock);
                services.AddSingleton(clock);
            });
    }
}
