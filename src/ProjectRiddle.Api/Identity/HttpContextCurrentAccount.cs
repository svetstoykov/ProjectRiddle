using System.Security.Claims;
using ProjectRiddle.Core.Interfaces.Accounts;

namespace ProjectRiddle.Api.Identity;

/// <summary>
/// Reads the current account identifier from the authenticated HTTP context.
/// </summary>
public sealed class HttpContextCurrentAccount : ICurrentAccount
{
    /// <summary>
    /// Initializes the current-account adapter.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor. Cannot be <see langword="null" />.</param>
    public HttpContextCurrentAccount(IHttpContextAccessor httpContextAccessor)
    {
        ArgumentNullException.ThrowIfNull(httpContextAccessor);

        var principal = httpContextAccessor.HttpContext?.User;
        if (principal?.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var raw = principal.FindFirstValue(ClaimTypes.NameIdentifier);
        if (Guid.TryParse(raw, out var accountId) && accountId != Guid.Empty)
        {
            AccountId = accountId;
        }
    }

    /// <inheritdoc />
    public Guid? AccountId { get; }
}
