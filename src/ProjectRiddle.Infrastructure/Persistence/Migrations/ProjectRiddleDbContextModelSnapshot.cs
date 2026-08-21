using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Infrastructure.Persistence.Migrations;

/// <summary>
/// Stores the model snapshot used by future EF Core migrations.
/// </summary>
[DbContext(typeof(ProjectRiddleDbContext))]
internal sealed partial class ProjectRiddleDbContextModelSnapshot : ModelSnapshot
{
    /// <inheritdoc />
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
        modelBuilder
            .HasAnnotation("ProductVersion", "10.0.11")
            .HasAnnotation("Relational:MaxIdentifierLength", 64);
    }
}
