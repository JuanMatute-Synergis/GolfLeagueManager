using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSessionBasedFlightAssignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PlayerFlightAssignments_PlayerId",
                table: "PlayerFlightAssignments");

            migrationBuilder.AddColumn<DateTime>(
                name: "AssignmentDate",
                table: "PlayerFlightAssignments",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<Guid>(
                name: "SeasonId",
                table: "PlayerFlightAssignments",
                type: "uuid",
                nullable: true); // Make nullable initially

            migrationBuilder.AddColumn<int>(
                name: "SessionStartWeekNumber",
                table: "PlayerFlightAssignments",
                type: "integer",
                nullable: false,
                defaultValue: 1); // Default to session/week 1

            // Set AssignmentDate to current timestamp for existing records
            migrationBuilder.Sql("UPDATE \"PlayerFlightAssignments\" SET \"AssignmentDate\" = NOW() WHERE \"AssignmentDate\" = '0001-01-01 00:00:00+00'");

            // Populate SeasonId from Flight table for existing records
            migrationBuilder.Sql(@"
                UPDATE ""PlayerFlightAssignments"" 
                SET ""SeasonId"" = f.""SeasonId""
                FROM ""Flights"" f 
                WHERE ""PlayerFlightAssignments"".""FlightId"" = f.""Id"" 
                AND f.""SeasonId"" IS NOT NULL");

            // Delete any PlayerFlightAssignments that reference flights without seasons
            migrationBuilder.Sql(@"
                DELETE FROM ""PlayerFlightAssignments"" 
                WHERE ""FlightId"" IN (
                    SELECT ""Id"" FROM ""Flights"" WHERE ""SeasonId"" IS NULL
                )");

            // Delete any PlayerFlightAssignments that still have null SeasonId
            migrationBuilder.Sql("DELETE FROM \"PlayerFlightAssignments\" WHERE \"SeasonId\" IS NULL");

            // Now make SeasonId NOT NULL
            migrationBuilder.AlterColumn<Guid>(
                name: "SeasonId",
                table: "PlayerFlightAssignments",
                type: "uuid",
                nullable: false);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFlightAssignment_PlayerSeasonSession",
                table: "PlayerFlightAssignments",
                columns: new[] { "PlayerId", "SeasonId", "SessionStartWeekNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFlightAssignments_SeasonId",
                table: "PlayerFlightAssignments",
                column: "SeasonId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlayerFlightAssignments_Seasons_SeasonId",
                table: "PlayerFlightAssignments",
                column: "SeasonId",
                principalTable: "Seasons",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlayerFlightAssignments_Seasons_SeasonId",
                table: "PlayerFlightAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PlayerFlightAssignment_PlayerSeasonSession",
                table: "PlayerFlightAssignments");

            migrationBuilder.DropIndex(
                name: "IX_PlayerFlightAssignments_SeasonId",
                table: "PlayerFlightAssignments");

            migrationBuilder.DropColumn(
                name: "AssignmentDate",
                table: "PlayerFlightAssignments");

            migrationBuilder.DropColumn(
                name: "SeasonId",
                table: "PlayerFlightAssignments");

            migrationBuilder.DropColumn(
                name: "SessionStartWeekNumber",
                table: "PlayerFlightAssignments");

            migrationBuilder.CreateIndex(
                name: "IX_PlayerFlightAssignments_PlayerId",
                table: "PlayerFlightAssignments",
                column: "PlayerId");
        }
    }
}
