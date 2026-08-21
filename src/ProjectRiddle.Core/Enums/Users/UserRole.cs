namespace ProjectRiddle.Core.Enums.Users;

/// <summary>
/// Defines the V1 local-account roles.
/// </summary>
public enum UserRole
{
    /// <summary>
    /// Indicates a self-registered account without administrative access.
    /// </summary>
    User = 0,

    /// <summary>
    /// Indicates an administrator account provisioned through the protected bootstrap or administrative boundary.
    /// </summary>
    Admin = 1
}
