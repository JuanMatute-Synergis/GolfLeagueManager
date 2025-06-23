using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class AddSpecialPointsToWeek : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SpecialCircumstanceNote",
                table: "Weeks");

            migrationBuilder.DropColumn(
                name: "SpecialPointsAwarded",
                table: "Weeks");
        }
    }
}
