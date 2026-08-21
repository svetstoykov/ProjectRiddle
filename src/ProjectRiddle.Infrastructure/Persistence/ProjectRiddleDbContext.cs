using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectRiddle.Infrastructure.Identity;

namespace ProjectRiddle.Infrastructure.Persistence;

/// <summary>
/// Provides the EF Core persistence boundary for Project Riddle, including ASP.NET Identity.
/// </summary>
public sealed class ProjectRiddleDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    /// <summary>
    /// Initializes the persistence context.
    /// </summary>
    /// <param name="options">The options configured for the context.</param>
    public ProjectRiddleDbContext(DbContextOptions<ProjectRiddleDbContext> options)
        : base(options)
    {
    }

    /// <inheritdoc />
    protected override void OnModelCreating(ModelBuilder builder)
    {
        ArgumentNullException.ThrowIfNull(builder);
        base.OnModelCreating(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(ProjectRiddleDbContext).Assembly);
    }
}
