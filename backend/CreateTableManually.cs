using Microsoft.EntityFrameworkCore;
using GolfLeagueManager;
using System;
using System.Threading.Tasks;

var connectionString = "Host=localhost;Database=golfdb_default;Username=postgres;Password=yourpassword";
var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
optionsBuilder.UseNpgsql(connectionString);

using var context = new AppDbContext(optionsBuilder.Options);

try
{
    var sql = @"
        CREATE TABLE IF NOT EXISTS ""LeagueSettings"" (
            ""Id"" uuid NOT NULL DEFAULT gen_random_uuid(),
            ""SeasonId"" uuid NOT NULL,
            ""HandicapMethod"" integer NOT NULL,
            ""CoursePar"" integer NOT NULL,
            ""CourseRating"" numeric NOT NULL,
            ""SlopeRating"" numeric NOT NULL,
            ""MaxRoundsForHandicap"" integer NOT NULL,
            ""ScoringMethod"" integer NOT NULL,
            ""PointsSystem"" integer NOT NULL,
            ""HoleWinPoints"" integer NOT NULL,
            ""HoleHalvePoints"" integer NOT NULL,
            ""MatchWinBonus"" integer NOT NULL,
            ""MatchTiePoints"" integer NOT NULL,
            ""UseSessionHandicaps"" boolean NOT NULL,
            ""AllowHandicapUpdates"" boolean NOT NULL,
            ""CustomRules"" character varying(2000),
            ""CreatedDate"" timestamp with time zone NOT NULL,
            ""ModifiedDate"" timestamp with time zone,
            CONSTRAINT ""PK_LeagueSettings"" PRIMARY KEY (""Id""),
            CONSTRAINT ""FK_LeagueSettings_Seasons_SeasonId"" FOREIGN KEY (""SeasonId"") 
                REFERENCES ""Seasons"" (""Id"") ON DELETE CASCADE
        );
        
        CREATE INDEX IF NOT EXISTS ""IX_LeagueSettings_SeasonId"" ON ""LeagueSettings"" (""SeasonId"");
        CREATE UNIQUE INDEX IF NOT EXISTS ""IX_LeagueSettings_SeasonId_Unique"" ON ""LeagueSettings"" (""SeasonId"");
    ";
    
    await context.Database.ExecuteSqlRawAsync(sql);
    Console.WriteLine("LeagueSettings table created successfully!");
}
catch (Exception ex)
{
    Console.WriteLine($"Error creating table: {ex.Message}");
    Console.WriteLine($"Stack trace: {ex.StackTrace}");
}
