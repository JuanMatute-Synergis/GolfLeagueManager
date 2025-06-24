using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionStartToWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PlayerHandicapHistories");

            migrationBuilder.DropColumn(
                name: "SpecialCircumstanceNote",
                table: "Weeks");

            migrationBuilder.DropColumn(
                name: "SpecialPointsAwarded",
                table: "Weeks");

            migrationBuilder.AddColumn<bool>(
                name: "SessionStart",
                table: "Weeks",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SessionStart",
                table: "Weeks");

            migrationBuilder.AddColumn<string>(
                name: "SpecialCircumstanceNote",
                table: "Weeks",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SpecialPointsAwarded",
                table: "Weeks",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PlayerHandicapHistories",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlayerId = table.Column<Guid>(type: "uuid", nullable: false),
                    WeekId = table.Column<Guid>(type: "uuid", nullable: false),
                    AverageScore = table.Column<double>(type: "double precision", nullable: false),
                    DateRecorded = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Handicap = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlayerHandicapHistories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlayerHandicapHistories_Players_PlayerId",
                        column: x => x.PlayerId,
                        principalTable: "Players",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlayerHandicapHistories_Weeks_WeekId",
                        column: x => x.WeekId,
                        principalTable: "Weeks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlayerHandicapHistories_PlayerId",
                table: "PlayerHandicapHistories",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerHandicapHistories_WeekId",
                table: "PlayerHandicapHistories",
                column: "WeekId");
        }
    }
}
