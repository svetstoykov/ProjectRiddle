using ProjectRiddle.Core.Enums.Users;

namespace ProjectRiddle.Api.Models.Auth;

/// <summary>
/// Represents the authenticated session returned by account operations.
/// </summary>
public sealed record SessionResponse
{
    /// <summary>
    /// Gets the stable account identifier.
    /// </summary>
    public required Guid Id { get; init; }

    /// <summary>
    /// Gets the display email address.
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// Gets the assigned role.
    /// </summary>
    public required UserRole Role { get; init; }
}
