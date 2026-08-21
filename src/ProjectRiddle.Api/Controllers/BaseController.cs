using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using ProjectRiddle.Core.Results.Models;

namespace ProjectRiddle.Api.Controllers;

/// <summary>
/// Provides the single API mapping point from Core failures to Problem Details responses.
/// </summary>
public abstract class BaseController : ControllerBase
{
    /// <summary>
    /// Maps a Core failure to a standard Problem Details response.
    /// </summary>
    /// <param name="error">The expected Core failure. Cannot be <see langword="null" />.</param>
    /// <returns>A Problem Details action result with the status mapped from <paramref name="error" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error" /> is <see langword="null" />.</exception>
    protected ActionResult FromFailure(OperationError error)
    {
        ArgumentNullException.ThrowIfNull(error);

        var statusCode = MapStatusCode(error.Type);
        var traceId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
        var problemDetails = new ProblemDetails
        {
            Type = $"https://httpstatuses.com/{statusCode}",
            Title = GetTitle(error.Type),
            Status = statusCode,
            Detail = error.Message,
            Instance = HttpContext.Request.Path
        };

        problemDetails.Extensions["traceId"] = traceId;

        if (error.Code is not null)
        {
            problemDetails.Extensions["code"] = error.Code;
        }

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode,
            ContentTypes = new MediaTypeCollection
            {
                "application/problem+json"
            }
        };
    }

    /// <summary>
    /// Maps a Core failure to a standard Problem Details response for a typed action.
    /// </summary>
    /// <typeparam name="TResponse">The successful response type of the calling action.</typeparam>
    /// <param name="error">The expected Core failure. Cannot be <see langword="null" />.</param>
    /// <returns>A Problem Details action result with the status mapped from <paramref name="error" />.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="error" /> is <see langword="null" />.</exception>
    protected ActionResult<TResponse> FromFailure<TResponse>(OperationError error)
    {
        return FromFailure(error);
    }

    private static int MapStatusCode(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.MalformedInput => StatusCodes.Status400BadRequest,
            ErrorType.UnprocessableInput => StatusCodes.Status422UnprocessableEntity,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.InvalidOperation => StatusCodes.Status409Conflict,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.Timeout => StatusCodes.Status504GatewayTimeout,
            ErrorType.ExternalDependencyFailure => StatusCodes.Status503ServiceUnavailable,
            ErrorType.InternalError => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    private static string GetTitle(ErrorType errorType)
    {
        return errorType switch
        {
            ErrorType.NotFound => "Resource not found",
            ErrorType.Validation => "Validation failed",
            ErrorType.MalformedInput => "Malformed input",
            ErrorType.UnprocessableInput => "Input cannot be processed",
            ErrorType.Conflict => "Conflict",
            ErrorType.InvalidOperation => "Invalid operation",
            ErrorType.Unauthorized => "Authentication required",
            ErrorType.Forbidden => "Forbidden",
            ErrorType.Timeout => "Operation timed out",
            ErrorType.ExternalDependencyFailure => "External dependency failure",
            ErrorType.InternalError => "Internal error",
            _ => "Request failed"
        };
    }
}
