using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddAbsenceScenarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PlayerAAbsent",
                table: "Matchups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PlayerAAbsentWithNotice",
                table: "Matchups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PlayerBAbsent",
                table: "Matchups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "PlayerBAbsentWithNotice",
                table: "Matchups",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerAAbsent",
                table: "Matchups");

            migrationBuilder.DropColumn(
                name: "PlayerAAbsentWithNotice",
                table: "Matchups");

            migrationBuilder.DropColumn(
                name: "PlayerBAbsent",
                table: "Matchups");

            migrationBuilder.DropColumn(
                name: "PlayerBAbsentWithNotice",
                table: "Matchups");
        }
    }
}
