using ProjectRiddle.Core.Enums.Users;
using ProjectRiddle.Core.Models.Users;

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

    /// <summary>
    /// Maps a Core registration output to the session response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static SessionResponse FromCoreRegisterUserOutput(RegisterUserOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new SessionResponse
        {
            Id = output.Id,
            Email = output.Email,
            Role = output.Role
        };
    }

    /// <summary>
    /// Maps a Core authentication output to the session response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static SessionResponse FromCoreAuthenticateUserOutput(AuthenticateUserOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new SessionResponse
        {
            Id = output.Id,
            Email = output.Email,
            Role = output.Role
        };
    }

    /// <summary>
    /// Maps a Core current-session output to the session response.
    /// </summary>
    /// <param name="output">The Core output. Cannot be <see langword="null" />.</param>
    /// <returns>The corresponding API response.</returns>
    public static SessionResponse FromCoreCurrentSessionOutput(CurrentSessionOutput output)
    {
        ArgumentNullException.ThrowIfNull(output);
        return new SessionResponse
        {
            Id = output.Id,
            Email = output.Email,
            Role = output.Role
        };
    }
}
