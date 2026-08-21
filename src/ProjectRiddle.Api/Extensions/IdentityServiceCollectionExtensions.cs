using Microsoft.AspNetCore.Identity;
using ProjectRiddle.Api.Authorization;
using ProjectRiddle.Infrastructure.Identity;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Api.Extensions;

/// <summary>
/// Registers ASP.NET Identity and cookie authentication at the composition boundary.
/// </summary>
public static class IdentityServiceCollectionExtensions
{
    /// <summary>
    /// Adds Identity stores, token providers, and the application authentication cookie.
    /// </summary>
    /// <param name="services">The service collection to configure. Cannot be <see langword="null" />.</param>
    /// <returns>The supplied service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    public static IServiceCollection AddProjectRiddleIdentity(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

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

        return services;
    }
}
