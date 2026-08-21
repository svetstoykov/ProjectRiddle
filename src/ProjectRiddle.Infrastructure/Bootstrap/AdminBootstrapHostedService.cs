using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ProjectRiddle.Infrastructure.Configuration;
using ProjectRiddle.Infrastructure.Identity;

namespace ProjectRiddle.Infrastructure.Bootstrap;

/// <summary>
/// Ensures Identity roles exist and provisions the first administrator from runtime-only configuration.
/// </summary>
public sealed partial class AdminBootstrapHostedService : IHostedService
{
    private const string UserRoleName = "user";
    private const string AdminRoleName = "admin";

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
        await using var scope = scopeFactory.CreateAsyncScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        await EnsureRoleExistsAsync(roleManager, UserRoleName);
        await EnsureRoleExistsAsync(roleManager, AdminRoleName);

        var email = options.Value.Email;
        var password = options.Value.Password;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            return;
        }

        var existing = await userManager.FindByEmailAsync(email.Trim());
        if (existing is not null)
        {
            LogBootstrapSkipped(logger);
            return;
        }

        var administrator = new ApplicationUser
        {
            UserName = email.Trim(),
            Email = email.Trim()
        };
        var created = await userManager.CreateAsync(administrator, password);
        if (!created.Succeeded)
        {
            throw new InvalidOperationException("Administrator bootstrap failed to create the configured account.");
        }

        var roleResult = await userManager.AddToRoleAsync(administrator, AdminRoleName);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException("Administrator bootstrap failed to assign the admin role.");
        }

        LogBootstrapApplied(logger, administrator.Id);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private static async Task EnsureRoleExistsAsync(RoleManager<ApplicationRole> roleManager, string roleName)
    {
        if (await roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var created = await roleManager.CreateAsync(new ApplicationRole(roleName));
        if (!created.Succeeded)
        {
            throw new InvalidOperationException($"Failed to create the '{roleName}' role.");
        }
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
