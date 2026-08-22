using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectRiddle.Infrastructure.Persistence.Migrations
{
    /// <summary>
    /// Adds account-owned riddle progress, hint, and revealed-position tables.
    /// </summary>
    internal sealed partial class AddRiddleProgress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RiddleProgress",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    AccountId = table.Column<Guid>(type: "TEXT", nullable: false),
                    RiddleId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    AnswerAttemptCount = table.Column<int>(type: "INTEGER", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiddleProgress", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RiddleProgress_AspNetUsers_AccountId",
                        column: x => x.AccountId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RiddleProgress_Riddles_RiddleId",
                        column: x => x.RiddleId,
                        principalTable: "Riddles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiddleProgressHints",
                columns: table => new
                {
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    RiddleProgressId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiddleProgressHints", x => new { x.RiddleProgressId, x.Kind });
                    table.ForeignKey(
                        name: "FK_RiddleProgressHints_RiddleProgress_RiddleProgressId",
                        column: x => x.RiddleProgressId,
                        principalTable: "RiddleProgress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RiddleProgressPositions",
                columns: table => new
                {
                    LetterPosition = table.Column<int>(type: "INTEGER", nullable: false),
                    RiddleProgressId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RiddleProgressPositions", x => new { x.RiddleProgressId, x.LetterPosition });
                    table.ForeignKey(
                        name: "FK_RiddleProgressPositions_RiddleProgress_RiddleProgressId",
                        column: x => x.RiddleProgressId,
                        principalTable: "RiddleProgress",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

#pragma warning disable CA1861
            migrationBuilder.CreateIndex(
                name: "IX_RiddleProgress_AccountId_RiddleId",
                table: "RiddleProgress",
                columns: new[] { "AccountId", "RiddleId" },
                unique: true);
#pragma warning restore CA1861

            migrationBuilder.CreateIndex(
                name: "IX_RiddleProgress_RiddleId",
                table: "RiddleProgress",
                column: "RiddleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RiddleProgressHints");

            migrationBuilder.DropTable(
                name: "RiddleProgressPositions");

            migrationBuilder.DropTable(
                name: "RiddleProgress");
        }
    }
}
