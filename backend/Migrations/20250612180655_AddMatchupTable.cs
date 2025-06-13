using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchupTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Matchups",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    WeekId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerAId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerBId = table.Column<Guid>(type: "uuid", nullable: false),
                    PlayerAScore = table.Column<int>(type: "integer", nullable: true),
                    PlayerBScore = table.Column<int>(type: "integer", nullable: true),
                    PlayerAPoints = table.Column<int>(type: "integer", nullable: true),
                    PlayerBPoints = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Matchups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Matchups_Players_PlayerAId",
                        column: x => x.PlayerAId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matchups_Players_PlayerBId",
                        column: x => x.PlayerBId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Matchups_Weeks_WeekId",
                        column: x => x.WeekId,
                        principalTable: "Weeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Matchups_PlayerAId",
                table: "Matchups",
                column: "PlayerAId");

            migrationBuilder.CreateIndex(
                name: "IX_Matchups_PlayerBId",
                table: "Matchups",
                column: "PlayerBId");

            migrationBuilder.CreateIndex(
                name: "IX_Matchups_WeekId",
                table: "Matchups",
                column: "WeekId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Matchups");
        }
    }
}
