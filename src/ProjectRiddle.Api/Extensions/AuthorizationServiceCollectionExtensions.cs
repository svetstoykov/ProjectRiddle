using Microsoft.AspNetCore.Authorization;
using ProjectRiddle.Api.Authorization;

namespace ProjectRiddle.Api.Extensions;

/// <summary>
/// Registers authorization policies at the composition boundary.
/// </summary>
public static class AuthorizationServiceCollectionExtensions
{
    /// <summary>
    /// Adds the authenticated fallback policy and named administrative policy.
    /// </summary>
    /// <param name="services">The service collection to configure. Cannot be <see langword="null" />.</param>
    /// <returns>The supplied service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    public static IServiceCollection AddProjectRiddleAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddAuthorization(options =>
        {
            options.FallbackPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();
            options.AddPolicy(
                AuthorizationPolicies.Admin,
                policy => policy.RequireRole(RoleClaimValues.Admin));
        });

        return services;
    }
}
