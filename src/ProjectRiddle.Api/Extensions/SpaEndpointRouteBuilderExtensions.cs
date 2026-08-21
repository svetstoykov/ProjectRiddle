using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ProjectRiddle.Api.Extensions;

/// <summary>
/// Maps the single-host API and compiled SPA boundary.
/// </summary>
public static class SpaEndpointRouteBuilderExtensions
{
    /// <summary>
    /// Maps the compiled SPA fallback while keeping unmatched API routes in Problem Details format.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <param name="environment">The web host environment containing the compiled frontend.</param>
    /// <returns>The mapped fallback endpoint convention builder.</returns>
    public static IEndpointConventionBuilder MapProjectRiddleSpaFallback(
        this IEndpointRouteBuilder endpoints,
        IWebHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(endpoints);
        ArgumentNullException.ThrowIfNull(environment);

        return endpoints.MapFallback(
            async context =>
            {
                if (context.Request.Path.StartsWithSegments("/api"))
                {
                    await WriteNotFoundProblemDetailsAsync(context);
                    return;
                }

                if (string.IsNullOrWhiteSpace(environment.WebRootPath))
                {
                    await WriteNotFoundProblemDetailsAsync(context);
                    return;
                }

                var indexPath = Path.Combine(environment.WebRootPath, "index.html");

                if (!File.Exists(indexPath))
                {
                    await WriteNotFoundProblemDetailsAsync(context);
                    return;
                }

                context.Response.ContentType = "text/html";
                await context.Response.SendFileAsync(indexPath);
            });
    }

    private static async Task WriteNotFoundProblemDetailsAsync(HttpContext context)
    {
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        var problemDetails = new ProblemDetails
        {
            Type = "https://httpstatuses.com/404",
            Title = "Resource not found",
            Status = StatusCodes.Status404NotFound,
            Detail = "The requested resource was not found.",
            Instance = context.Request.Path
        };
        problemDetails.Extensions["traceId"] = traceId;

        context.Response.StatusCode = StatusCodes.Status404NotFound;
        context.Response.ContentType = "application/problem+json";
        await JsonSerializer.SerializeAsync(context.Response.Body, problemDetails);
    }
}
