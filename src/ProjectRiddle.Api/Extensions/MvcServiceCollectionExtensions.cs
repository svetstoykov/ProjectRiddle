using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectRiddle.Api.Authorization;
using ProjectRiddle.Api.Identity;
using ProjectRiddle.Api.Infrastructure;
using ProjectRiddle.Core.Interfaces.Accounts;

namespace ProjectRiddle.Api.Extensions;

/// <summary>
/// Registers API delivery services at the composition boundary.
/// </summary>
public static class MvcServiceCollectionExtensions
{
    /// <summary>
    /// Adds controllers, Problem Details, exception handling, and CSRF protection.
    /// </summary>
    /// <param name="services">The service collection to configure. Cannot be <see langword="null" />.</param>
    /// <returns>The supplied service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    public static IServiceCollection AddProjectRiddleMvc(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentAccount, HttpContextCurrentAccount>();
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

        return services;
    }
}
