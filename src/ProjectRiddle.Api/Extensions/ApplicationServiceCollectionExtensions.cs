using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Services.Riddles;
using ProjectRiddle.Core.Services.System;

namespace ProjectRiddle.Api.Extensions;

/// <summary>
/// Registers Core application services at the composition boundary.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Adds Core service implementations used by API controllers.
    /// </summary>
    /// <param name="services">The service collection to configure. Cannot be <see langword="null" />.</param>
    /// <returns>The supplied service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="services" /> is <see langword="null" />.</exception>
    public static IServiceCollection AddProjectRiddleApplicationServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IInternalStatusService, InternalStatusService>();
        services.AddScoped<IAdminRiddlesService, AdminRiddlesService>();
        services.AddScoped<IRiddlesService, RiddlesService>();

        return services;
    }
}
