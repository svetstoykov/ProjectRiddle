using ProjectRiddle.Core.Enums.Users;

namespace ProjectRiddle.Api.Authorization;

/// <summary>
/// Converts between Core roles and authentication role claim values.
/// </summary>
public static class RoleClaimValues
{
    /// <summary>
    /// Gets the role claim value for a self-registered account.
    /// </summary>
    public const string User = "user";

    /// <summary>
    /// Gets the role claim value for an administrator.
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// Converts a Core role to the matching claim value.
    /// </summary>
    /// <param name="role">The Core role.</param>
    /// <returns>The claim value stored on the authentication cookie.</returns>
    public static string FromRole(UserRole role)
    {
        return role == UserRole.Admin ? Admin : User;
    }

    /// <summary>
    /// Converts a role claim value to a Core role.
    /// </summary>
    /// <param name="value">The claim value.</param>
    /// <returns>The matching Core role when recognized; otherwise <see langword="null" />.</returns>
    public static UserRole? ToRole(string? value)
    {
        return value switch
        {
            Admin => UserRole.Admin,
            User => UserRole.User,
            _ => null
        };
    }
}
