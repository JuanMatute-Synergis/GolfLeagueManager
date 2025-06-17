using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHoleScoreTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "HoleScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    MatchupId = table.Column<Guid>(type: "uuid", nullable: false),
                    HoleNumber = table.Column<int>(type: "integer", nullable: false),
                    Par = table.Column<int>(type: "integer", nullable: false),
                    PlayerAScore = table.Column<int>(type: "integer", nullable: true),
                    PlayerBScore = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HoleScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HoleScores_Matchups_MatchupId",
                        column: x => x.MatchupId,
                        principalTable: "Matchups",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HoleScores_MatchupId_HoleNumber",
                table: "HoleScores",
                columns: new[] { "MatchupId", "HoleNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HoleScores");
        }
    }
}
