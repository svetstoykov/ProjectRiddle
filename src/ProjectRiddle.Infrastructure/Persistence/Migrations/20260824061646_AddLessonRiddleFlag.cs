using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectRiddle.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Adds the flag that separates course lesson riddles from daily riddles.
    /// </summary>
    public partial class AddLessonRiddleFlag : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsLesson",
                table: "Riddles",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Riddles_IsLesson",
                table: "Riddles",
                column: "IsLesson");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Riddles_IsLesson",
                table: "Riddles");

            migrationBuilder.DropColumn(
                name: "IsLesson",
                table: "Riddles");
        }
    }
}
