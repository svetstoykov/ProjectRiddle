using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ProjectRiddle.Api.Authorization;
using ProjectRiddle.Api.Models.Auth;
using ProjectRiddle.Core.Results.Models;
using ProjectRiddle.Infrastructure.Identity;

namespace ProjectRiddle.Api.Controllers;

/// <summary>
/// Exposes registration, sign-in, sign-out, current-session, and CSRF token operations.
/// </summary>
[ApiController]
[Route("api/auth")]
public sealed class AuthController : BaseController
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IAntiforgery _antiforgery;
    private readonly ILogger<AuthController> _logger;

    /// <summary>
    /// Initializes the authentication controller.
    /// </summary>
    /// <param name="userManager">The ASP.NET Identity user manager.</param>
    /// <param name="signInManager">The ASP.NET Identity sign-in manager.</param>
    /// <param name="antiforgery">The antiforgery service used to issue CSRF request tokens.</param>
    /// <param name="logger">The logger for safe account lifecycle events.</param>
    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IAntiforgery antiforgery,
        ILogger<AuthController> logger)
    {
        ArgumentNullException.ThrowIfNull(userManager);
        ArgumentNullException.ThrowIfNull(signInManager);
        ArgumentNullException.ThrowIfNull(antiforgery);
        ArgumentNullException.ThrowIfNull(logger);

        this._userManager = userManager;
        this._signInManager = signInManager;
        this._antiforgery = antiforgery;
        this._logger = logger;
    }

    /// <summary>
    /// Registers a local account with the user role.
    /// </summary>
    /// <param name="request">The registration request.</param>
    /// <returns>The created account when registration succeeds.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the user role cannot be assigned after the account is created.</exception>
    [HttpPost("register")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SessionResponse>> RegisterAsync([FromBody] RegisterRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim()
        };
        var created = await _userManager.CreateAsync(user, request.Password);
        if (!created.Succeeded)
        {
            return FromIdentityFailure(created);
        }

        var roleResult = await _userManager.AddToRoleAsync(user, RoleClaimValues.User);
        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException("The registered account could not be assigned the user role.");
        }

        _logger.LogInformation("Registered a local user account. UserId: {UserId}", user.Id);
        return Created("/api/auth/session", CreateSessionResponse(user, [RoleClaimValues.User]));
    }

    /// <summary>
    /// Verifies credentials and establishes a cookie session.
    /// </summary>
    /// <param name="request">The sign-in request.</param>
    /// <returns>The authenticated session when credentials are valid.</returns>
    [HttpPost("sign-in")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionResponse>> SignInAsync([FromBody] SignInRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return FromFailure<SessionResponse>(InvalidCredentials());
        }

        var user = await _userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null)
        {
            return FromFailure<SessionResponse>(InvalidCredentials());
        }

        var signIn = await _signInManager.CheckPasswordSignInAsync(
            user,
            request.Password,
            lockoutOnFailure: false);
        if (!signIn.Succeeded)
        {
            return FromFailure<SessionResponse>(InvalidCredentials());
        }

        await _signInManager.SignInAsync(user, isPersistent: true);
        var roles = await _userManager.GetRolesAsync(user);
        return Ok(CreateSessionResponse(user, roles));
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
        await _signInManager.SignOutAsync();
        return NoContent();
    }

    /// <summary>
    /// Gets the account for the current cookie session.
    /// </summary>
    /// <returns>The current session when the caller is authenticated.</returns>
    [HttpGet("session")]
    [ProducesResponseType(typeof(SessionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<SessionResponse>> GetSessionAsync()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user is null)
        {
            return FromFailure<SessionResponse>(
                new OperationError(
                    "Authentication is required.",
                    ErrorType.Unauthorized,
                    UserErrorCodes.Unauthorized));
        }

        var roles = await _userManager.GetRolesAsync(user);
        return Ok(CreateSessionResponse(user, roles));
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
        var tokens = _antiforgery.GetAndStoreTokens(HttpContext);
        return Ok(new AntiforgeryTokenResponse { Token = tokens.RequestToken! });
    }

    private static SessionResponse CreateSessionResponse(ApplicationUser user, IEnumerable<string> roles)
    {
        return new SessionResponse
        {
            Id = user.Id,
            Email = user.Email!,
            Role = RoleClaimValues.ToUserRole(roles)
        };
    }

    private ActionResult<SessionResponse> FromIdentityFailure(IdentityResult result)
    {
        var code = result.Errors.Select(error => error.Code).FirstOrDefault();
        if (code is "DuplicateEmail" or "DuplicateUserName")
        {
            return FromFailure<SessionResponse>(
                new OperationError(
                    "An account with this email address already exists.",
                    ErrorType.Conflict,
                    UserErrorCodes.EmailConflict));
        }

        if (code is not null && code.StartsWith("Password", StringComparison.Ordinal))
        {
            return FromFailure<SessionResponse>(
                new OperationError(
                    "Password must be between 8 and 256 characters.",
                    ErrorType.Validation,
                    UserErrorCodes.PasswordInvalid));
        }

        return FromFailure<SessionResponse>(
            new OperationError(
                "A valid email address is required.",
                ErrorType.Validation,
                UserErrorCodes.EmailInvalid));
    }

    private static OperationError InvalidCredentials()
    {
        return new OperationError(
            "Invalid email or password.",
            ErrorType.Unauthorized,
            UserErrorCodes.CredentialsInvalid);
    }
}
