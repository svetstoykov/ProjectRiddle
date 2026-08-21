using Microsoft.AspNetCore.Identity;

namespace ProjectRiddle.Infrastructure.Identity;

/// <summary>
/// Represents an ASP.NET Identity account stored by the application.
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    /// <summary>
    /// Initializes an account with a new identifier.
    /// </summary>
    public ApplicationUser()
    {
        Id = Guid.NewGuid();
    }
}
