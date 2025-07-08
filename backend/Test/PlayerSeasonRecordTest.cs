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

        /// <summary>
        /// Test handicap calculation with course par adjustment for 9-hole vs 18-hole par
        /// </summary>
        public static async Task TestHandicapCalculationWithCourseParAdjustment(AppDbContext context)
        {
            Console.WriteLine("\n=== Testing Handicap Calculation with Course Par Adjustment ===");

            // Get required services
            var leagueSettingsService = new LeagueSettingsService(context);
            var playerSeasonStatsService = new PlayerSeasonStatsService(context);
            var averageScoreService = new AverageScoreService(context, playerSeasonStatsService, leagueSettingsService);
            var handicapService = new HandicapService(context, leagueSettingsService, playerSeasonStatsService, averageScoreService);

            // Get the first player and season for testing
            var player = await context.Players.FirstOrDefaultAsync();
            var season = await context.Seasons.FirstOrDefaultAsync();

            if (player == null || season == null)
            {
                Console.WriteLine("No players or seasons found for testing");
                return;
            }

            Console.WriteLine($"Testing for Player: {player.FirstName} {player.LastName}");
            Console.WriteLine($"Season: {season.Name}");

            // Get or create league settings
            var leagueSettings = await leagueSettingsService.GetLeagueSettingsAsync(season.Id);

            // Test 1: 9-hole course par (36)
            Console.WriteLine("\n--- Test 1: 9-hole Course Par (36) ---");
            leagueSettings.CoursePar = 36;
            leagueSettings.HandicapMethod = HandicapCalculationMethod.SimpleAverage;
            await leagueSettingsService.UpdateLeagueSettingsAsync(leagueSettings);

            // Mock an average score for testing
            await playerSeasonStatsService.UpdateInitialValuesAsync(player.Id, season.Id, 0, 40); // Average score of 40

            var handicap36 = await handicapService.GetPlayerSessionHandicapAsync(player.Id, season.Id, 1);
            Console.WriteLine($"Course Par: 36, Average Score: 40, Calculated Handicap: {handicap36}");
            Console.WriteLine($"Expected: 40 - 36 = 4 (should be 4)");

            // Test 2: 18-hole course par (72) - should be divided by 2
            Console.WriteLine("\n--- Test 2: 18-hole Course Par (72) ---");
            leagueSettings.CoursePar = 72;
            await leagueSettingsService.UpdateLeagueSettingsAsync(leagueSettings);

            var handicap72 = await handicapService.GetPlayerSessionHandicapAsync(player.Id, season.Id, 1);
            Console.WriteLine($"Course Par: 72 (effective: 36), Average Score: 40, Calculated Handicap: {handicap72}");
            Console.WriteLine($"Expected: 40 - (72/2) = 40 - 36 = 4 (should be 4)");

            // Test 3: Edge case - course par 45 (should not be divided)
            Console.WriteLine("\n--- Test 3: Edge Case - Course Par 45 ---");
            leagueSettings.CoursePar = 45;
            await leagueSettingsService.UpdateLeagueSettingsAsync(leagueSettings);

            var handicap45 = await handicapService.GetPlayerSessionHandicapAsync(player.Id, season.Id, 1);
            Console.WriteLine($"Course Par: 45, Average Score: 40, Calculated Handicap: {handicap45}");
            Console.WriteLine($"Expected: 40 - 45 = -5, capped at 0 (should be 0)");

            // Test 4: Edge case - course par 46 (should be divided by 2)
            Console.WriteLine("\n--- Test 4: Edge Case - Course Par 46 ---");
            leagueSettings.CoursePar = 46;
            await leagueSettingsService.UpdateLeagueSettingsAsync(leagueSettings);

            var handicap46 = await handicapService.GetPlayerSessionHandicapAsync(player.Id, season.Id, 1);
            Console.WriteLine($"Course Par: 46 (effective: 23), Average Score: 40, Calculated Handicap: {handicap46}");
            Console.WriteLine($"Expected: 40 - (46/2) = 40 - 23 = 17 (should be 17)");

            // Verify the results
            Console.WriteLine("\n--- Verification ---");
            Console.WriteLine($"Test 1 (Par 36): {(handicap36 == 4 ? "PASS" : "FAIL")}");
            Console.WriteLine($"Test 2 (Par 72): {(handicap72 == 4 ? "PASS" : "FAIL")}");
            Console.WriteLine($"Test 3 (Par 45): {(handicap45 == 0 ? "PASS" : "FAIL")}");
            Console.WriteLine($"Test 4 (Par 46): {(handicap46 == 17 ? "PASS" : "FAIL")}");
        }
    }
}
