using ProjectRiddle.Core.Enums.Users;

namespace ProjectRiddle.Core.Interfaces.Users;

/// <summary>
/// Provides the authenticated caller identity to Core without exposing delivery-layer types.
/// </summary>
public interface ICurrentUser
{
    /// <summary>
    /// Gets a value indicating whether the caller is authenticated.
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Gets the caller's user identifier when the caller is authenticated.
    /// </summary>
    Guid? UserId { get; }

    /// <summary>
    /// Gets the caller's role when the caller is authenticated and the role claim is recognized.
    /// </summary>
    UserRole? Role { get; }
}
