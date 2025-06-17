using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchPlayScoring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PlayerAHolePoints",
                table: "Matchups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PlayerAMatchWin",
                table: "Matchups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PlayerBHolePoints",
                table: "Matchups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "PlayerBMatchWin",
                table: "Matchups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "HoleHandicap",
                table: "HoleScores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlayerAMatchPoints",
                table: "HoleScores",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "PlayerBMatchPoints",
                table: "HoleScores",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlayerAHolePoints",
                table: "Matchups");

            migrationBuilder.DropColumn(
                name: "PlayerAMatchWin",
                table: "Matchups");

            migrationBuilder.DropColumn(
                name: "PlayerBHolePoints",
                table: "Matchups");

            migrationBuilder.DropColumn(
                name: "PlayerBMatchWin",
                table: "Matchups");

            migrationBuilder.DropColumn(
                name: "HoleHandicap",
                table: "HoleScores");

            migrationBuilder.DropColumn(
                name: "PlayerAMatchPoints",
                table: "HoleScores");

            migrationBuilder.DropColumn(
                name: "PlayerBMatchPoints",
                table: "HoleScores");
        }
    }
}
