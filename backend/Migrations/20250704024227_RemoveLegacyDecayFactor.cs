using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class RemoveLegacyDecayFactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacyDecayFactor",
                table: "LeagueSettings");

            migrationBuilder.AddColumn<int>(
                name: "LegacyInitialWeight",
                table: "LeagueSettings",
                type: "integer",
                nullable: false,
                defaultValue: 4);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LegacyInitialWeight",
                table: "LeagueSettings");

            migrationBuilder.AddColumn<decimal>(
                name: "LegacyDecayFactor",
                table: "LeagueSettings",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
