namespace ProjectRiddle.Api.Models.Auth;

/// <summary>
/// Represents the request token used by cookie-authenticated state-changing requests.
/// </summary>
public sealed record AntiforgeryTokenResponse
{
    /// <summary>
    /// Gets the request token that must be sent in the CSRF header.
    /// </summary>
    public required string Token { get; init; }
}
