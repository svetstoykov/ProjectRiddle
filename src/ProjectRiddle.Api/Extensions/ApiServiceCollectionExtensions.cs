using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using ProjectRiddle.Api.Authorization;
using ProjectRiddle.Api.Infrastructure;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Services.Riddles;
using ProjectRiddle.Core.Services.System;
using ProjectRiddle.Infrastructure.Configuration;
using ProjectRiddle.Infrastructure.Identity;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Api.Extensions;

/// <summary>
/// Registers API delivery and Core application services at the composition boundary.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds controllers, Problem Details, authentication, authorization, and Core services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The supplied service collection.</returns>
    public static IServiceCollection AddProjectRiddleApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHttpContextAccessor();
        services.AddControllers(options =>
            {
                options.Filters.Add<AutoValidateAntiforgeryFilter>();
            })
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(
                    new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
            });

        services.AddAntiforgery(options =>
        {
            options.HeaderName = "X-CSRF-TOKEN";
            options.Cookie.Name = "ProjectRiddle.Csrf";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        });

        services
            .AddIdentity<ApplicationUser, ApplicationRole>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 0;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Lockout.AllowedForNewUsers = false;
            })
            .AddEntityFrameworkStores<ProjectRiddleDbContext>()
            .AddDefaultTokenProviders();

        services.ConfigureApplicationCookie(options =>
        {
            options.Cookie.Name = "ProjectRiddle.Auth";
            options.Cookie.HttpOnly = true;
            options.Cookie.SameSite = SameSiteMode.Lax;
            options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
            options.SlidingExpiration = true;
            options.ExpireTimeSpan = TimeSpan.FromDays(14);
            options.Events.OnRedirectToLogin = CookieAuthenticationProblemDetails.WriteUnauthorizedAsync;
            options.Events.OnRedirectToAccessDenied = CookieAuthenticationProblemDetails.WriteForbiddenAsync;
        });

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.AddPolicy(
                AuthorizationPolicies.Admin,
                policy => policy.RequireRole(RoleClaimValues.Admin));
        });

        services.AddDataProtection()
            .SetApplicationName("ProjectRiddle");
        services.AddSingleton<IConfigureOptions<KeyManagementOptions>>(serviceProvider =>
        {
            return new ConfigureOptions<KeyManagementOptions>(options =>
            {
                options.XmlRepository = CreateKeyRepository(serviceProvider);
            });
        });

        services.AddSingleton<IInternalStatusService, InternalStatusService>();
        services.AddScoped<IRiddlesService, RiddlesService>();

        return services;
    }

    private static FileSystemXmlRepository CreateKeyRepository(IServiceProvider serviceProvider)
    {
        var databaseOptions = serviceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
        var environment = serviceProvider.GetRequiredService<IHostEnvironment>();
        var databasePath = Path.GetFullPath(databaseOptions.DatabasePath, environment.ContentRootPath);
        var databaseDirectory = Path.GetDirectoryName(databasePath) ?? environment.ContentRootPath;
        var keysPath = Path.Combine(databaseDirectory, "keys");
        Directory.CreateDirectory(keysPath);
        var loggerFactory = serviceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
        return new FileSystemXmlRepository(new DirectoryInfo(keysPath), loggerFactory);
    }
}
