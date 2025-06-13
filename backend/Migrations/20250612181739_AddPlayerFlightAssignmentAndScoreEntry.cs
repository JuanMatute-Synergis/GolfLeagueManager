using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerFlightAssignmentAndScoreEntry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerFlightAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    FlightId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsFlightLeader = table.Column<bool>(type: "boolean", nullable: false),
                    HandicapAtAssignment = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerFlightAssignments", x => x.Id);
                    // Temporarily only add the Player foreign key
                    table.ForeignKey(
                        name: "FK_PlayerFlightAssignments_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScoreEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekId = table.Column<Guid>(type: "uuid", nullable: false),
                    Score = table.Column<int>(type: "integer", nullable: false),
                    PointsEarned = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScoreEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScoreEntries_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScoreEntries_Weeks_WeekId",
                        column: x => x.WeekId,
                        principalTable: "Weeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFlightAssignments_FlightId",
                table: "PlayerFlightAssignments",
                column: "FlightId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFlightAssignments_PlayerId",
                table: "PlayerFlightAssignments",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreEntries_PlayerId",
                table: "ScoreEntries",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_ScoreEntries_WeekId",
                table: "ScoreEntries",
                column: "WeekId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerFlightAssignments");

            migrationBuilder.DropTable(
                name: "ScoreEntries");
        }
    }
}
