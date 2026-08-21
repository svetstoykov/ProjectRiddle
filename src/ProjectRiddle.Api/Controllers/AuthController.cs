using System.Security.Claims;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectRiddle.Api.Authorization;
using ProjectRiddle.Api.Models.Auth;
using ProjectRiddle.Core.Interfaces.Services;

namespace ProjectRiddle.Api.Controllers;

/// <summary>
/// Exposes registration, sign-in, sign-out, current-session, and CSRF token operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : BaseController
{
    private readonly IUsersService usersService;
    private readonly IAntiforgery antiforgery;

    /// <summary>
    /// Initializes the authentication controller.
    /// </summary>
    /// <param name="usersService">The Core users service.</param>
    /// <param name="antiforgery">The antiforgery service used to issue CSRF request tokens.</param>
    public AuthController(IUsersService usersService, IAntiforgery antiforgery)
    {
        ArgumentNullException.ThrowIfNull(usersService);
        ArgumentNullException.ThrowIfNull(antiforgery);

        this.usersService = usersService;
        this.antiforgery = antiforgery;
    }

    /// <summary>
    /// Registers a local account with the user role.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The created account when registration succeeds.</returns>
    [HttpPost("register")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponse>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await usersService.RegisterAsync(request.ToCoreRegisterUserInput(), cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<SessionResponse>(result.Error!);
        }

        var response = SessionResponse.FromCoreRegisterUserOutput(result.Value!);
        return Created("/api/auth/session", response);
    }

    /// <summary>
    /// Verifies credentials and establishes a cookie session.
    /// </summary>
    /// <param name="request">The sign-in request.</param>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The authenticated session when credentials are valid.</returns>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionResponse>> SignInAsync(
        [FromBody] SignInRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var result = await usersService.AuthenticateAsync(request.ToCoreAuthenticateUserInput(), cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<SessionResponse>(result.Error!);
        }

        var user = result.Value!;
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, RoleClaimValues.FromRole(user.Role))
        };
        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity));

        return Ok(SessionResponse.FromCoreAuthenticateUserOutput(user));
    }

    /// <summary>
    /// Clears the current cookie session.
    /// </summary>
    /// <returns>A bodyless success response.</returns>
    [HttpPost("sign-out")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> SignOutAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return NoContent();
    }

    /// <summary>
    /// Gets the account for the current cookie session.
    /// </summary>
    /// <param name="cancellationToken">The token used to cancel the request.</param>
    /// <returns>The current session when the caller is authenticated.</returns>
    [HttpGet("session")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionResponse>> GetSessionAsync(CancellationToken cancellationToken)
    {
        var result = await usersService.GetCurrentAsync(cancellationToken);
        if (result.IsFailure)
        {
            return FromFailure<SessionResponse>(result.Error!);
        }

        return Ok(SessionResponse.FromCoreCurrentSessionOutput(result.Value!));
    }

    /// <summary>
    /// Issues a CSRF request token for cookie-authenticated state-changing requests.
    /// </summary>
    /// <returns>The request token and matching antiforgery cookie.</returns>
    [HttpGet("antiforgery")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(AntiforgeryTokenResponse), StatusCodes.Status200OK)]
    public ActionResult<AntiforgeryTokenResponse> GetAntiforgeryToken()
    {
        var tokens = antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new AntiforgeryTokenResponse { Token = tokens.RequestToken! });
    }
}
