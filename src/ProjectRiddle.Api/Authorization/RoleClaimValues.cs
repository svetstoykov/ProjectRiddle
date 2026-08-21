using ProjectRiddle.Core.Enums.Users;

namespace ProjectRiddle.Api.Authorization;

/// <summary>
/// Provides the ASP.NET Identity role names used by cookies and authorization policies.
/// </summary>
public static class RoleClaimValues
{
    /// <summary>
    /// Gets the role name for a self-registered account.
    /// </summary>
    public const string User = "user";

    /// <summary>
    /// Gets the role name for an administrator.
    /// </summary>
    public const string Admin = "admin";

    /// <summary>
    /// Maps Identity role names to the session role contract.
    /// </summary>
    /// <param name="roles">The role names assigned to the account. Cannot be <see langword="null" />.</param>
    /// <returns>The administrator role when present; otherwise the user role.</returns>
    public static UserRole ToUserRole(IEnumerable<string> roles)
    {
        ArgumentNullException.ThrowIfNull(roles);
        return roles.Contains(Admin, StringComparer.Ordinal) ? UserRole.Admin : UserRole.User;
    }
}
