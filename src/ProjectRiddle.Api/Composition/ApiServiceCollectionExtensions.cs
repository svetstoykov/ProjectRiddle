using Microsoft.Extensions.DependencyInjection;
using ProjectRiddle.Api.Infrastructure;
using ProjectRiddle.Core.Interfaces.Services;
using ProjectRiddle.Core.Services.Diagnostics;

namespace ProjectRiddle.Api.Composition;

/// <summary>
/// Registers API delivery and Core application services at the composition boundary.
/// </summary>
public static class ApiServiceCollectionExtensions
{
    /// <summary>
    /// Adds controllers, Problem Details, the global exception handler, and Phase 0 Core services.
    /// </summary>
    /// <param name="services">The service collection to configure.</param>
    /// <returns>The supplied service collection.</returns>
    public static IServiceCollection AddProjectRiddleApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddControllers();
        services.AddSingleton<IWalkingSkeletonService, WalkingSkeletonService>();

        return services;
    }
}
