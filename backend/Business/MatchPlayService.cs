using Microsoft.EntityFrameworkCore;
using GolfLeagueManager.Business;

namespace GolfLeagueManager
{
    public class MatchPlayService
    {
        private readonly AppDbContext _context;
        private readonly MatchPlayScoringService _scoringService;
        private readonly HandicapService _handicapService;
        private readonly LeagueSettingsService _leagueSettingsService;
        private readonly PlayerSeasonStatsService _playerSeasonStatsService;
        private readonly AverageScoreService _averageScoreService;

        public MatchPlayService(AppDbContext context, MatchPlayScoringService scoringService, HandicapService handicapService, LeagueSettingsService leagueSettingsService, PlayerSeasonStatsService playerSeasonStatsService, AverageScoreService averageScoreService)
        {
            _context = context;
            _scoringService = scoringService;
            _handicapService = handicapService;
            _leagueSettingsService = leagueSettingsService;
            _playerSeasonStatsService = playerSeasonStatsService;
            _averageScoreService = averageScoreService;
        }

        /// <summary>
        /// Calculate match play results for a matchup based on hole-by-hole scores
        /// </summary>
        public async Task<bool> CalculateMatchPlayResultsAsync(Guid matchupId)
        {
            Console.WriteLine($"[DEBUG] CalculateMatchPlayResultsAsync called for matchup {matchupId}");

            // Get matchup with players and hole scores
            var matchup = await _context.Matchups
                .Include(m => m.PlayerA)
                .Include(m => m.PlayerB)
                .FirstOrDefaultAsync(m => m.Id == matchupId);

            if (matchup == null)
            {
                Console.WriteLine($"[DEBUG] Matchup {matchupId} not found");
                return false;
            }

            Console.WriteLine($"[DEBUG] Matchup found: PlayerA={matchup.PlayerA?.FirstName} {matchup.PlayerA?.LastName}, PlayerB={matchup.PlayerB?.FirstName} {matchup.PlayerB?.LastName}");
            Console.WriteLine($"[DEBUG] Absence flags: PlayerAAbsent={matchup.PlayerAAbsent}, PlayerBAbsent={matchup.PlayerBAbsent}");

            // Handle absence scenarios first
            if (matchup.PlayerAAbsent || matchup.PlayerBAbsent)
            {
                Console.WriteLine("[DEBUG] Calling CalculateAbsenceScenarioAsync");
                return await CalculateAbsenceScenarioAsync(matchup);
            }

            Console.WriteLine("[DEBUG] No absence scenario detected, proceeding with normal match play calculation");

            // Get the week for session context
            var week = await _context.Weeks.FirstOrDefaultAsync(w => w.Id == matchup.WeekId);
            if (week == null)
            {
                Console.WriteLine("[DEBUG] Week not found for matchup");
                return false;
            }

            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchupId)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();

            if (!holeScores.Any())
            {
                Console.WriteLine("[DEBUG] No hole scores found");
                return false;
            }

            // Get scoring handicaps for both players (based on previous week's data)
            var playerAHandicap = await _handicapService.GetPlayerScoringHandicapAsync(matchup.PlayerAId, week.SeasonId, week.WeekNumber);
            var playerBHandicap = await _handicapService.GetPlayerScoringHandicapAsync(matchup.PlayerBId, week.SeasonId, week.WeekNumber);

            // Get league settings for the season
            var leagueSettings = await _leagueSettingsService.GetLeagueSettingsAsync(week.SeasonId);

            // Calculate gross totals for tie-breaking
            var playerAGrossTotal = holeScores.Where(hs => hs.PlayerAScore.HasValue).Sum(hs => hs.PlayerAScore!.Value);
            var playerBGrossTotal = holeScores.Where(hs => hs.PlayerBScore.HasValue).Sum(hs => hs.PlayerBScore!.Value);

            // Use the new match play scoring service with league settings
            var matchPlayResult = _scoringService.CalculateMatchPlayResult(
                holeScores,
                playerAHandicap,
                playerBHandicap,
                leagueSettings,
                playerAGrossTotal,
                playerBGrossTotal);

            // Update hole scores with match play points
            for (int i = 0; i < holeScores.Count; i++)
            {
                var holeResult = matchPlayResult.HoleResults.FirstOrDefault(hr => hr.HoleNumber == holeScores[i].HoleNumber);
                if (holeResult != null)
                {
                    holeScores[i].PlayerAMatchPoints = holeResult.PlayerAPoints;
                    holeScores[i].PlayerBMatchPoints = holeResult.PlayerBPoints;
                }
            }

            // Update matchup with match play results
            matchup.PlayerAHolePoints = matchPlayResult.PlayerAHolePoints;
            matchup.PlayerBHolePoints = matchPlayResult.PlayerBHolePoints;
            matchup.PlayerAPoints = matchPlayResult.PlayerATotalPoints;
            matchup.PlayerBPoints = matchPlayResult.PlayerBTotalPoints;
            matchup.PlayerAMatchWin = matchPlayResult.PlayerAMatchWin;
            matchup.PlayerBMatchWin = matchPlayResult.PlayerBMatchWin;

            // Special circumstance points logic (applies to both normal and absence scenarios)
            if (week.SpecialPointsAwarded.HasValue)
            {
                int special = week.SpecialPointsAwarded.Value;
                matchup.PlayerAPoints = matchup.PlayerAAbsent ? special / 2 : special;
                matchup.PlayerBPoints = matchup.PlayerBAbsent ? special / 2 : special;
                // Optionally, set hole points to 0 for special weeks
                matchup.PlayerAHolePoints = 0;
                matchup.PlayerBHolePoints = 0;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculate points for absence scenarios
        /// IMPORTANT: This method should NOT modify absence status (PlayerXAbsent or PlayerXAbsentWithNotice)
        /// Those values are set by the scorecard service and should be preserved
        /// </summary>
        private async Task<bool> CalculateAbsenceScenarioAsync(Matchup matchup)
        {
            Console.WriteLine($"[DEBUG] CalculateAbsenceScenarioAsync called for matchup {matchup.Id}");
            Console.WriteLine($"[DEBUG] PlayerAAbsent: {matchup.PlayerAAbsent}, PlayerBAbsent: {matchup.PlayerBAbsent}");
            Console.WriteLine($"[DEBUG] PlayerAAbsentWithNotice: {matchup.PlayerAAbsentWithNotice}, PlayerBAbsentWithNotice: {matchup.PlayerBAbsentWithNotice}");

            // PRESERVE the original absence status - do not modify these values
            // They are set by the scorecard service based on user input
            var originalPlayerAAbsent = matchup.PlayerAAbsent;
            var originalPlayerBAbsent = matchup.PlayerBAbsent;
            var originalPlayerAAbsentWithNotice = matchup.PlayerAAbsentWithNotice;
            var originalPlayerBAbsentWithNotice = matchup.PlayerBAbsentWithNotice;

            // Get the week and league settings for absence point calculations
            var week = await _context.Weeks.FirstOrDefaultAsync(w => w.Id == matchup.WeekId);
            if (week == null)
            {
                Console.WriteLine("[DEBUG] Week not found for absence scenario");
                return false;
            }

            var leagueSettings = await _leagueSettingsService.GetLeagueSettingsAsync(week.SeasonId);

            // Calculate absence notice points based on league settings
            // Standard absence with notice gets 1/5 of total possible points (20% of max points)
            var maxPossiblePoints = (leagueSettings.HoleWinPoints * 9) + leagueSettings.MatchWinBonus; // 9 holes + match bonus
            var absenceNoticePoints = Math.Max(1, maxPossiblePoints / 5); // At least 1 point, typically 4 for standard 2-1-2 system

            if (matchup.PlayerAAbsent && matchup.PlayerBAbsent)
            {
                Console.WriteLine("[DEBUG] Both players absent scenario");
                // Both absent - distribute points based on notice
                if (matchup.PlayerAAbsentWithNotice && matchup.PlayerBAbsentWithNotice)
                {
                    // Both gave notice - split points evenly
                    matchup.PlayerAPoints = absenceNoticePoints;
                    matchup.PlayerBPoints = absenceNoticePoints;
                    Console.WriteLine($"[DEBUG] Both gave notice: A={absenceNoticePoints}, B={absenceNoticePoints}");
                }
                else if (matchup.PlayerAAbsentWithNotice)
                {
                    // Only A gave notice
                    matchup.PlayerAPoints = absenceNoticePoints;
                    matchup.PlayerBPoints = 0;
                    Console.WriteLine($"[DEBUG] Only A gave notice: A={absenceNoticePoints}, B=0");
                }
                else if (matchup.PlayerBAbsentWithNotice)
                {
                    // Only B gave notice
                    matchup.PlayerAPoints = 0;
                    matchup.PlayerBPoints = absenceNoticePoints;
                    Console.WriteLine($"[DEBUG] Only B gave notice: A=0, B={absenceNoticePoints}");
                }
                else
                {
                    // Neither gave notice
                    matchup.PlayerAPoints = 0;
                    matchup.PlayerBPoints = 0;
                    Console.WriteLine("[DEBUG] Neither gave notice: A=0, B=0");
                }
                matchup.PlayerAHolePoints = 0;
                matchup.PlayerBHolePoints = 0;
                matchup.PlayerAMatchWin = false;
                matchup.PlayerBMatchWin = false;
            }
            else if (matchup.PlayerAAbsent)
            {
                Console.WriteLine("[DEBUG] Player A absent, Player B present");
                // Player A absent, Player B present
                var playerAPoints = matchup.PlayerAAbsentWithNotice ? absenceNoticePoints : 0;
                var playerBHandicap = await GetPlayerHandicapAsync(matchup.PlayerBId);
                var playerBPoints = await CalculateNoOpponentScoringAsync(matchup, matchup.PlayerBId, playerBHandicap, leagueSettings);

                Console.WriteLine($"[DEBUG] Player A points: {playerAPoints}, Player B points: {playerBPoints}");

                matchup.PlayerAPoints = playerAPoints;
                matchup.PlayerBPoints = playerBPoints;
                matchup.PlayerAHolePoints = 0;
                // In absence scenarios, no match win bonus is awarded - the points from CalculateNoOpponentScoringAsync are the total
                matchup.PlayerBHolePoints = playerBPoints; // All points are hole points in absence scenarios
                matchup.PlayerAMatchWin = false;
                matchup.PlayerBMatchWin = playerBPoints > playerAPoints;

                Console.WriteLine($"[DEBUG] Final Player A: Total={matchup.PlayerAPoints}, Hole={matchup.PlayerAHolePoints}, Win={matchup.PlayerAMatchWin}");
                Console.WriteLine($"[DEBUG] Final Player B: Total={matchup.PlayerBPoints}, Hole={matchup.PlayerBHolePoints}, Win={matchup.PlayerBMatchWin}");
            }
            else if (matchup.PlayerBAbsent)
            {
                Console.WriteLine("[DEBUG] Player B absent, Player A present");
                // Player B absent, Player A present
                var playerBPoints = matchup.PlayerBAbsentWithNotice ? absenceNoticePoints : 0;
                var playerAHandicap = await GetPlayerHandicapAsync(matchup.PlayerAId);
                var playerAPoints = await CalculateNoOpponentScoringAsync(matchup, matchup.PlayerAId, playerAHandicap, leagueSettings);

                Console.WriteLine($"[DEBUG] Player A points: {playerAPoints}, Player B points: {playerBPoints}");

                matchup.PlayerAPoints = playerAPoints;
                matchup.PlayerBPoints = playerBPoints;
                // In absence scenarios, no match win bonus is awarded - the points from CalculateNoOpponentScoringAsync are the total
                matchup.PlayerAHolePoints = playerAPoints; // All points are hole points in absence scenarios
                matchup.PlayerBHolePoints = 0;
                matchup.PlayerAMatchWin = playerAPoints > playerBPoints;
                matchup.PlayerBMatchWin = false;

                Console.WriteLine($"[DEBUG] Final Player A: Total={matchup.PlayerAPoints}, Hole={matchup.PlayerAHolePoints}, Win={matchup.PlayerAMatchWin}");
                Console.WriteLine($"[DEBUG] Final Player B: Total={matchup.PlayerBPoints}, Hole={matchup.PlayerBHolePoints}, Win={matchup.PlayerBMatchWin}");
            }

            Console.WriteLine($"[DEBUG] Final points after absence calculation: PlayerA={matchup.PlayerAPoints}, PlayerB={matchup.PlayerBPoints}");

            // CRITICAL: Restore the original absence status to prevent overwriting user input
            // The scorecard service has already set these values based on user selection
            matchup.PlayerAAbsent = originalPlayerAAbsent;
            matchup.PlayerBAbsent = originalPlayerBAbsent;
            matchup.PlayerAAbsentWithNotice = originalPlayerAAbsentWithNotice;
            matchup.PlayerBAbsentWithNotice = originalPlayerBAbsentWithNotice;

            Console.WriteLine($"[DEBUG] Preserved original absence status: PlayerAAbsentWithNotice={matchup.PlayerAAbsentWithNotice}, PlayerBAbsentWithNotice={matchup.PlayerBAbsentWithNotice}");

            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Calculate points when player has no opponent (absence scenario)
        /// Rules: Present player gets higher points if they beat their average by a whole number, otherwise lower points
        /// NO match win bonus is awarded in absence scenarios - only hole points
        /// </summary>
        private async Task<int> CalculateNoOpponentScoringAsync(Matchup matchup, Guid playerId, double handicap, LeagueSettings leagueSettings)
        {
            Console.WriteLine($"[DEBUG] CalculateNoOpponentScoringAsync called for player {playerId}");

            // Calculate scoring thresholds based on league settings
            // Maximum points: ONLY hole points, NO match win bonus in absence scenarios
            var maxHolePoints = leagueSettings.HoleWinPoints * 8; // Points for 8 holes (16 with standard 2-point system)
            var highScorePoints = maxHolePoints; // 16 points for beating average (NO match bonus in absence scenarios)
            var lowScorePoints = 8; // Fixed 8 points for not beating average in absence scenarios

            // Get the player
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                Console.WriteLine($"[DEBUG] Player {playerId} not found, returning default {lowScorePoints} points");
                return lowScorePoints; // Default fallback: low points
            }

            // Get seasonId and week number from the matchup's week
            var week = await _context.Weeks.FindAsync(matchup.WeekId);
            if (week == null)
            {
                Console.WriteLine($"[DEBUG] Week not found for matchup, returning default {lowScorePoints} points");
                return lowScorePoints;
            }

            var seasonId = week.SeasonId;
            var weekNumber = week.WeekNumber;

            // Get average score up to the PREVIOUS week for scoring calculations
            // This ensures we're using the average as of the start of this week, not including this week's score
            var previousWeekNumber = Math.Max(1, weekNumber - 1); // Don't go below week 1

            // Use AverageScoreService to get the correct average up to the previous week
            var averageScore = await _averageScoreService.UpdatePlayerAverageScoreAsync(playerId, seasonId, previousWeekNumber);

            Console.WriteLine($"[DEBUG] Player: {player.FirstName} {player.LastName}, Week: {weekNumber}, Previous Week Average: {averageScore}");

            // Get hole scores for this matchup to calculate total score
            var holeScores = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchup.Id)
                .OrderBy(hs => hs.HoleNumber)
                .ToListAsync();

            Console.WriteLine($"[DEBUG] Found {holeScores.Count} hole scores for matchup {matchup.Id}");

            if (!holeScores.Any())
            {
                Console.WriteLine($"[DEBUG] No hole scores available, returning {lowScorePoints} points");
                return lowScorePoints;
            }

            // Calculate the player's total gross score
            int totalScore = 0;
            bool hasValidScore = false;

            foreach (var holeScore in holeScores)
            {
                var actualScore = playerId == matchup.PlayerAId ? holeScore.PlayerAScore : holeScore.PlayerBScore;

                if (actualScore.HasValue)
                {
                    totalScore += actualScore.Value;
                    hasValidScore = true;
                    Console.WriteLine($"[DEBUG] Hole {holeScore.HoleNumber}: Score = {actualScore.Value}");
                }
            }

            Console.WriteLine($"[DEBUG] Total score: {totalScore}, HasValidScore: {hasValidScore}");

            if (!hasValidScore)
            {
                Console.WriteLine($"[DEBUG] No valid scores found, returning {lowScorePoints} points");
                return lowScorePoints;
            }

            // Check if player beat their average by a whole number
            // Example: If average is 43.99, they need to shoot 42 or better (less than 43)
            var requiredScore = Math.Floor(averageScore); // This gives us the whole number threshold

            Console.WriteLine($"[DEBUG] Player average: {averageScore}, Required score to beat average: less than {requiredScore}, Actual score: {totalScore}");

            int totalPoints;
            if (totalScore < requiredScore)
            {
                Console.WriteLine($"[DEBUG] Player beat their average by a whole number - awarding {highScorePoints} points ({maxHolePoints} hole points, NO match bonus in absence scenario)");
                totalPoints = highScorePoints;
            }
            else
            {
                Console.WriteLine($"[DEBUG] Player did not beat their average by a whole number - awarding {lowScorePoints} points");
                totalPoints = lowScorePoints;
            }

            Console.WriteLine($"[DEBUG] Final points for absence scenario: {totalPoints} (NO match win bonus awarded)");
            return totalPoints;
        }

        /// <summary>
        /// Get player's current handicap from their flight assignment
        /// </summary>
        private async Task<double> GetPlayerHandicapAsync(Guid playerId)
        {
            var assignment = await _context.PlayerFlightAssignments
                .Where(pfa => pfa.PlayerId == playerId)
                .OrderByDescending(pfa => pfa.Id) // Get most recent assignment
                .FirstOrDefaultAsync();

            return assignment?.HandicapAtAssignment ?? 0.0;
        }

        /// <summary>
        /// Initialize hole scores with default hole handicaps (stroke index)
        /// This assumes a standard 9-hole course with typical stroke index distribution
        /// </summary>
        public async Task<bool> InitializeHoleScoresAsync(Guid matchupId)
        {
            var existingHoles = await _context.HoleScores
                .Where(hs => hs.MatchupId == matchupId)
                .ToListAsync();

            if (existingHoles.Any()) return true; // Already initialized

            // Standard 9-hole stroke index (1 = hardest, 9 = easiest)
            var defaultHoleHandicaps = new Dictionary<int, int>
            {
                { 1, 1 }, { 2, 5 }, { 3, 3 }, { 4, 7 }, { 5, 2 },
                { 6, 8 }, { 7, 4 }, { 8, 6 }, { 9, 9 }
            };

            // Standard 9-hole pars
            var defaultPars = new Dictionary<int, int>
            {
                { 1, 4 }, { 2, 3 }, { 3, 4 }, { 4, 5 }, { 5, 4 },
                { 6, 3 }, { 7, 4 }, { 8, 4 }, { 9, 5 }
            };

            for (int holeNumber = 1; holeNumber <= 9; holeNumber++)
            {
                var holeScore = new HoleScore
                {
                    Id = Guid.NewGuid(),
                    MatchupId = matchupId,
                    HoleNumber = holeNumber,
                    Par = defaultPars[holeNumber],
                    HoleHandicap = defaultHoleHandicaps[holeNumber],
                    PlayerAMatchPoints = 0,
                    PlayerBMatchPoints = 0
                };

                _context.HoleScores.Add(holeScore);
            }

            await _context.SaveChangesAsync();
            return true;
        }
    }
}
