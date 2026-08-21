using System.Diagnostics;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace ProjectRiddle.Api.Authorization;

/// <summary>
/// Writes Problem Details for cookie authentication challenges and forbidden responses.
/// </summary>
public static class CookieAuthenticationProblemDetails
{
    /// <summary>
    /// Writes a 401 Problem Details response instead of redirecting to a login page.
    /// </summary>
    /// <param name="context">The cookie redirect context.</param>
    /// <returns>A task that represents the write operation.</returns>
    public static Task WriteUnauthorizedAsync(RedirectContext<CookieAuthenticationOptions> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return WriteAsync(
            context.HttpContext,
            StatusCodes.Status401Unauthorized,
            "Authentication required",
            "Authentication is required.");
    }

    /// <summary>
    /// Writes a 403 Problem Details response instead of redirecting to an access-denied page.
    /// </summary>
    /// <param name="context">The cookie redirect context.</param>
    /// <returns>A task that represents the write operation.</returns>
    public static Task WriteForbiddenAsync(RedirectContext<CookieAuthenticationOptions> context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return WriteAsync(
            context.HttpContext,
            StatusCodes.Status403Forbidden,
            "Forbidden",
            "The caller is not permitted to perform this operation.");
    }

    private static Task WriteAsync(HttpContext httpContext, int statusCode, string title, string detail)
    {
        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = title,
            Status = statusCode,
            Detail = detail,
            Instance = httpContext.Request.Path
        };
        problemDetails.Extensions["traceId"] = traceId;

        httpContext.Response.StatusCode = statusCode;
        httpContext.Response.ContentType = "application/problem+json";
        return JsonSerializer.SerializeAsync(httpContext.Response.Body, problemDetails);
    }
}
