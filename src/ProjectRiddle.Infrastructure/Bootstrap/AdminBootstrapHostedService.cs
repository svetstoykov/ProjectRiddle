using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.Core.Interfaces.Repositories;
using ProjectRiddle.Core.Interfaces.Time;
using ProjectRiddle.Core.Interfaces.Users;
using ProjectRiddle.Core.Models.Users;
using ProjectRiddle.Core.Services.Users;
using ProjectRiddle.Infrastructure.Configuration;

namespace ProjectRiddle.Infrastructure.Bootstrap;

/// <summary>
/// Provisions the first administrator from runtime-only configuration without overwriting an existing account.
/// </summary>
public sealed partial class AdminBootstrapHostedService : IHostedService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IOptions<AdminBootstrapOptions> options;
    private readonly ILogger<AdminBootstrapHostedService> logger;

    /// <summary>
    /// Initializes the administrator bootstrap hosted service.
    /// </summary>
    /// <param name="scopeFactory">The factory used to resolve scoped persistence services.</param>
    /// <param name="options">The runtime-only bootstrap settings.</param>
    /// <param name="logger">The logger for safe bootstrap outcomes.</param>
    public AdminBootstrapHostedService(
        IServiceScopeFactory scopeFactory,
        IOptions<AdminBootstrapOptions> options,
        ILogger<AdminBootstrapHostedService> logger)
    {
        ArgumentNullException.ThrowIfNull(scopeFactory);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        this.scopeFactory = scopeFactory;
        this.options = options;
        this.logger = logger;
    }

    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var email = options.Value.Email;
        var password = options.Value.Password;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        var dateTimeProvider = scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        var normalizedEmail = EmailNormalizer.Normalize(email);
        var existing = await userRepository.GetByNormalizedEmailAsync(normalizedEmail, cancellationToken);
        if (existing is not null)
        {
            LogBootstrapSkipped(logger);
            return;
        }

        var administrator = new User(
            Guid.NewGuid(),
            email.Trim(),
            normalizedEmail,
            passwordHasher.HashPassword(password),
            UserRole.Admin,
            dateTimeProvider.UtcDateTime);

        await userRepository.AddAsync(administrator, cancellationToken);
        LogBootstrapApplied(logger, administrator.Id);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    [LoggerMessage(
        EventId = 2200,
        Level = LogLevel.Information,
        Message = "Administrator bootstrap created a new admin account. UserId: {UserId}")]
    private static partial void LogBootstrapApplied(ILogger logger, Guid userId);

    [LoggerMessage(
        EventId = 2201,
        Level = LogLevel.Information,
        Message = "Administrator bootstrap skipped because the configured account already exists.")]
    private static partial void LogBootstrapSkipped(ILogger logger);
}
