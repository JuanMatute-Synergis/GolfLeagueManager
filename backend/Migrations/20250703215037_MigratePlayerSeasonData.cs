using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace backend.Migrations
{
    /// <inheritdoc />
    public partial class MigratePlayerSeasonData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Create PlayerSeasonRecords for all existing players across all seasons
            // For each player in each season, copy their InitialHandicap and InitialAverageScore
            migrationBuilder.Sql(@"
                INSERT INTO ""PlayerSeasonRecords"" (""Id"", ""PlayerId"", ""SeasonId"", ""InitialHandicap"", ""InitialAverageScore"", ""CurrentHandicap"", ""CurrentAverageScore"", ""CreatedAt"", ""UpdatedAt"")
                SELECT 
                    gen_random_uuid() as ""Id"",
                    p.""Id"" as ""PlayerId"",
                    s.""Id"" as ""SeasonId"",
                    p.""InitialHandicap"" as ""InitialHandicap"",
                    p.""InitialAverageScore"" as ""InitialAverageScore"",
                    p.""InitialHandicap"" as ""CurrentHandicap"",
                    p.""CurrentAverageScore"" as ""CurrentAverageScore"",
                    NOW() as ""CreatedAt"",
                    NOW() as ""UpdatedAt""
                FROM ""Players"" p
                CROSS JOIN ""Seasons"" s
                WHERE EXISTS (
                    -- Only create records for players who have flight assignments in this season
                    SELECT 1 FROM ""PlayerFlightAssignments"" pfa
                    INNER JOIN ""Flights"" f ON pfa.""FlightId"" = f.""Id""
                    WHERE pfa.""PlayerId"" = p.""Id"" AND f.""SeasonId"" = s.""Id""
                )
                ON CONFLICT DO NOTHING;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove all PlayerSeasonRecords that were created during this migration
            migrationBuilder.Sql(@"DELETE FROM ""PlayerSeasonRecords"";");
        }
    }
}
