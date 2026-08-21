using System.Security.Claims;
using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.Core.Interfaces.Users;

namespace ProjectRiddle.Api.Authorization;

/// <summary>
/// Reads the current caller identity from the HTTP authentication cookie.
/// </summary>
public sealed class HttpCurrentUser : ICurrentUser
{
    private readonly IHttpContextAccessor httpContextAccessor;

    /// <summary>
    /// Initializes the current-user adapter.
    /// </summary>
    /// <param name="httpContextAccessor">The accessor for the current HTTP context.</param>
    public HttpCurrentUser(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);
        this.httpContextAccessor = httpContextAccessor;
    }

    /// <inheritdoc />
    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User.Identity?.IsAuthenticated == true;

    /// <inheritdoc />
    public Guid? UserId
    {
        get
        {
            var value = httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.TryParse(value, out var userId) ? userId : null;
        }
    }

    /// <inheritdoc />
    public UserRole? Role => RoleClaimValues.ToRole(httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Role));
}
