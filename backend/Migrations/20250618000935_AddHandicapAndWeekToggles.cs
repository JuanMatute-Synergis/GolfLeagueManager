using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddHandicapAndWeekToggles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "CountsForHandicap",
                table: "Weeks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "CountsForScoring",
                table: "Weeks",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "CurrentHandicap",
                table: "Players",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "InitialHandicap",
                table: "Players",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountsForHandicap",
                table: "Weeks");

            migrationBuilder.DropColumn(
                name: "CountsForScoring",
                table: "Weeks");

            migrationBuilder.DropColumn(
                name: "CurrentHandicap",
                table: "Players");

            migrationBuilder.DropColumn(
                name: "InitialHandicap",
                table: "Players");
        }
    }
}
