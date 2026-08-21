using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace ProjectRiddle.Api.Authorization;

/// <summary>
/// Validates the CSRF token for unsafe HTTP methods unless an action opts out.
/// </summary>
public sealed class AutoValidateAntiforgeryFilter : IAsyncAuthorizationFilter
{
    private readonly IAntiforgery _antiforgery;

    /// <summary>
    /// Initializes the CSRF validation filter.
    /// </summary>
    /// <param name="antiforgery">The antiforgery service.</param>
    public AutoValidateAntiforgeryFilter(IAntiforgery antiforgery)
    {
        ArgumentNullException.ThrowIfNull(antiforgery);
        this._antiforgery = antiforgery;
    }

    /// <inheritdoc />
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        if (IsSafeMethod(context.HttpContext.Request.Method)
            || context.Filters.OfType<IgnoreAntiforgeryTokenAttribute>().Any())
        {
            return;
        }

        try
        {
            await _antiforgery.ValidateRequestAsync(context.HttpContext);
        }
        catch (AntiforgeryValidationException)
        {
            context.Result = new ObjectResult(
                new ProblemDetails
                {
                    Type = "https://httpstatuses.com/400",
                    Title = "Invalid request token",
                    Status = StatusCodes.Status400BadRequest,
                    Detail = "The CSRF request token is missing or invalid.",
                    Instance = context.HttpContext.Request.Path
                })
            {
                StatusCode = StatusCodes.Status400BadRequest,
                ContentTypes = { "application/problem+json" }
            };
        }
    }

    private static bool IsSafeMethod(string method)
    {
        return HttpMethods.IsGet(method)
            || HttpMethods.IsHead(method)
            || HttpMethods.IsOptions(method)
            || HttpMethods.IsTrace(method);
    }
}
