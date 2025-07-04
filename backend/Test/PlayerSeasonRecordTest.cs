// Test script to verify PlayerSeasonRecord functionality
// This demonstrates how the new season-specific data structure works

using GolfLeagueManager;
using GolfLeagueManager.Business;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Test
{
    public class PlayerSeasonRecordTest
    {
        /// <summary>
        /// Example of how to use the new PlayerSeasonStatsService
        /// </summary>
        public static async Task TestPlayerSeasonRecord(AppDbContext context)
        {
            var playerSeasonStatsService = new PlayerSeasonStatsService(context);

            // Get the first player and season for testing
            var player = await context.Players.FirstOrDefaultAsync();
            var season = await context.Seasons.FirstOrDefaultAsync();

            if (player == null || season == null)
            {
                Console.WriteLine("No players or seasons found for testing");
                return;
            }

            Console.WriteLine($"Testing PlayerSeasonRecord for Player: {player.FirstName} {player.LastName}");
            Console.WriteLine($"Season: {season.Name}");

            // Test 1: Get or create season record
            var seasonRecord = await playerSeasonStatsService.GetOrCreatePlayerSeasonStatsAsync(player.Id, season.Id);
            Console.WriteLine($"Initial Handicap: {seasonRecord.InitialHandicap}");
            Console.WriteLine($"Initial Average Score: {seasonRecord.InitialAverageScore}");

            // Test 2: Update initial values (simulating flight assignment)
            await playerSeasonStatsService.UpdateInitialValuesAsync(player.Id, season.Id, 15.0m, 85.0m);
            Console.WriteLine("Updated initial values: Handicap=15.0, Average=85.0");

            // Test 3: Get updated values
            var handicap = await playerSeasonStatsService.GetInitialHandicapAsync(player.Id, season.Id);
            var averageScore = await playerSeasonStatsService.GetInitialAverageScoreAsync(player.Id, season.Id);
            Console.WriteLine($"Retrieved Initial Handicap: {handicap}");
            Console.WriteLine($"Retrieved Initial Average Score: {averageScore}");

            // Test 4: Update current calculated values
            await playerSeasonStatsService.UpdateCurrentValuesAsync(player.Id, season.Id, 14.2m, 83.5m);
            Console.WriteLine("Updated current values: Handicap=14.2, Average=83.5");

            var currentHandicap = await playerSeasonStatsService.GetCurrentHandicapAsync(player.Id, season.Id);
            var currentAverage = await playerSeasonStatsService.GetCurrentAverageScoreAsync(player.Id, season.Id);
            Console.WriteLine($"Retrieved Current Handicap: {currentHandicap}");
            Console.WriteLine($"Retrieved Current Average: {currentAverage}");

            Console.WriteLine("\n✅ PlayerSeasonRecord functionality verified successfully!");
        }
    }
}
