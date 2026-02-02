using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LifeGames.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Board",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Board", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BoardGeneration",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BoardId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GenerationNumber = table.Column<int>(type: "int", nullable: false),
                    Cells = table.Column<string>(type: "json", nullable: false),
                    ComputedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BoardGeneration", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BoardGeneration_Board_BoardId",
                        column: x => x.BoardId,
                        principalTable: "Board",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Board_CreatedAt",
                table: "Board",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_BoardGeneration_BoardId",
                table: "BoardGeneration",
                column: "BoardId");

            migrationBuilder.CreateIndex(
                name: "IX_BoardGeneration_BoardId_GenerationNumber",
                table: "BoardGeneration",
                columns: new[] { "BoardId", "GenerationNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BoardGeneration");

            migrationBuilder.DropTable(
                name: "Board");
        }
    }
}
