using Microsoft.EntityFrameworkCore;

namespace ProjectRiddle.Infrastructure.Persistence;

/// <summary>
/// Provides the EF Core persistence boundary for Project Riddle.
/// </summary>
public sealed class ProjectRiddleDbContext : DbContext
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
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        ArgumentNullException.ThrowIfNull(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectRiddleDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
