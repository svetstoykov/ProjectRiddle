using Microsoft.AspNetCore.Identity;

namespace ProjectRiddle.Infrastructure.Identity;

/// <summary>
/// Represents an ASP.NET Identity role stored by the application.
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    /// <summary>
    /// Initializes a role with a new identifier.
    /// </summary>
    public ApplicationRole()
    {
        Id = Guid.NewGuid();
    }

    /// <summary>
    /// Initializes a role with a new identifier and the supplied name.
    /// </summary>
    /// <param name="roleName">The role name. Cannot be <see langword="null" />.</param>
    public ApplicationRole(string roleName)
        : this()
    {
        Name = roleName;
    }
}
