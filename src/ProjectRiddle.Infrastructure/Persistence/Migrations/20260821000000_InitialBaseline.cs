using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using ProjectRiddle.Infrastructure.Persistence;

namespace ProjectRiddle.Infrastructure.Persistence.Migrations;

/// <summary>
/// Creates the initial EF Core migration history boundary.
/// </summary>
[DbContext(typeof(ProjectRiddleDbContext))]
[Migration("20260821000000_InitialBaseline")]
internal sealed partial class InitialBaseline : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}
