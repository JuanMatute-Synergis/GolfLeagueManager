using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLegacyInitialWeightToOne : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Update existing records from 4 to 1
            migrationBuilder.Sql("UPDATE \"LeagueSettings\" SET \"LegacyInitialWeight\" = 1 WHERE \"LegacyInitialWeight\" = 4;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert back to 4 if needed
            migrationBuilder.Sql("UPDATE \"LeagueSettings\" SET \"LegacyInitialWeight\" = 4 WHERE \"LegacyInitialWeight\" = 1;");
        }
    }
}
