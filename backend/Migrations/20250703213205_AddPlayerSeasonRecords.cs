using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddPlayerSeasonRecords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PlayerSeasonRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    SeasonId = table.Column<Guid>(type: "uuid", nullable: false),
                    InitialHandicap = table.Column<decimal>(type: "numeric", nullable: false),
                    InitialAverageScore = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentHandicap = table.Column<decimal>(type: "numeric", nullable: false),
                    CurrentAverageScore = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerSeasonRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonRecords_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerSeasonRecords_Seasons_SeasonId",
                        column: x => x.SeasonId,
                        principalTable: "Seasons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonRecords_PlayerId_SeasonId",
                table: "PlayerSeasonRecords",
                columns: new[] { "PlayerId", "SeasonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerSeasonRecords_SeasonId",
                table: "PlayerSeasonRecords",
                column: "SeasonId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerSeasonRecords");
        }
    }
}
