using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddLeagueSettings : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LeagueSettings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    HandicapMethod = table.Column<int>(type: "integer", nullable: false),
                    CoursePar = table.Column<int>(type: "integer", nullable: false),
                    CourseRating = table.Column<decimal>(type: "numeric", nullable: false),
                    SlopeRating = table.Column<decimal>(type: "numeric", nullable: false),
                    MaxRoundsForHandicap = table.Column<int>(type: "integer", nullable: false),
                    ScoringMethod = table.Column<int>(type: "integer", nullable: false),
                    PointsSystem = table.Column<int>(type: "integer", nullable: false),
                    HoleWinPoints = table.Column<int>(type: "integer", nullable: false),
                    HoleHalvePoints = table.Column<int>(type: "integer", nullable: false),
                    MatchWinBonus = table.Column<int>(type: "integer", nullable: false),
                    MatchTiePoints = table.Column<int>(type: "integer", nullable: false),
                    UseSessionHandicaps = table.Column<bool>(type: "boolean", nullable: false),
                    AllowHandicapUpdates = table.Column<bool>(type: "boolean", nullable: false),
                    CustomRules = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ModifiedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeagueSettings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LeagueSettings_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LeagueSettings_SeasonId",
                table: "LeagueSettings",
                column: "SeasonId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LeagueSettings");
        }
    }
}
