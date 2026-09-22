using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectRiddle.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Adds the optimistic-concurrency version used when a riddle is saved.
    /// </summary>
    internal sealed partial class AddRiddleConcurrencyVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "Riddles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "Riddles");
        }
    }
}
