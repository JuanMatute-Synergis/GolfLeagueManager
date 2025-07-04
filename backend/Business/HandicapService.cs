using Microsoft.EntityFrameworkCore;
using GolfLeagueManager.Business;

namespace GolfLeagueManager
{
    public class HandicapService
    {
        private readonly AppDbContext _context;
        private readonly LeagueSettingsService _leagueSettingsService;
        private readonly PlayerSeasonStatsService _playerSeasonStatsService;

        public HandicapService(AppDbContext context, LeagueSettingsService leagueSettingsService, PlayerSeasonStatsService playerSeasonStatsService)
        {
            _context = context;
            _leagueSettingsService = leagueSettingsService;
            _playerSeasonStatsService = playerSeasonStatsService;
        }

        /// <summary>
        /// Get a player's handicap for calculations. This method now calculates the handicap
        /// based on scores up to and including the specified week.
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID</param>
        /// <param name="weekNumber">The week number to get handicap for</param>
        /// <returns>The player's calculated handicap</returns>
        public async Task<decimal> GetPlayerSessionHandicapAsync(Guid playerId, Guid seasonId, int weekNumber)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} not found");

            // Calculate handicap based on scores up to and including the specified week
            return await GetPlayerHandicapUpToWeekAsync(playerId, seasonId, weekNumber);
        }

        /// <summary>
        /// Calculate a player's handicap based on scores up to and including a specific week
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID</param>
        /// <param name="upToWeekNumber">The week number to calculate handicap up to (inclusive)</param>
        /// <param name="maxRounds">Maximum number of recent rounds to consider (default 20)</param>
        /// <returns>The calculated handicap up to the specified week</returns>
        public async Task<decimal> GetPlayerHandicapUpToWeekAsync(Guid playerId, Guid seasonId, int upToWeekNumber, int maxRounds = 20)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} not found");

            // Get league settings to determine calculation method
            var leagueSettings = await _leagueSettingsService.GetLeagueSettingsAsync(seasonId);

            // Get ALL weeks in the season up to and including the specified week (both counting and non-counting)
            var allSeasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.CountsForScoring &&
                           w.WeekNumber <= upToWeekNumber &&
                           !w.SpecialPointsAwarded.HasValue) // Exclude weeks with special points
                .OrderBy(w => w.WeekNumber)
                .ToListAsync();

            // Build scores array using previous valid handicap for non-counting weeks
            var scoresForCalculation = new List<(int score, int courseRating, decimal slopeRating)>();
            var currentValidHandicap = await _playerSeasonStatsService.GetInitialHandicapAsync(playerId, seasonId); // This only updates when we have a counting week with actual score

            foreach (var week in allSeasonWeeks)
            {
                if (week.CountsForHandicap)
                {
                    // Get actual score for this week
                    var actualScore = await _context.Matchups
                        .Where(m => m.WeekId == week.Id &&
                                   (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                                   ((m.PlayerAId == playerId && m.PlayerAScore.HasValue && !m.PlayerAAbsent) ||
                                    (m.PlayerBId == playerId && m.PlayerBScore.HasValue && !m.PlayerBAbsent)))
                        .Select(m => m.PlayerAId == playerId ? m.PlayerAScore!.Value : m.PlayerBScore!.Value)
                        .FirstOrDefaultAsync();

                    if (actualScore > 0)
                    {
                        scoresForCalculation.Add((actualScore, (int)leagueSettings.CourseRating, leagueSettings.SlopeRating));

                        // Update the valid handicap AFTER adding this score for future non-counting weeks
                        // Calculate handicap based on all scores accumulated so far
                        if (leagueSettings.HandicapMethod == HandicapCalculationMethod.SimpleAverage)
                        {
                            var avgScore = (decimal)scoresForCalculation.Average(s => s.score);
                            currentValidHandicap = Math.Round(avgScore - leagueSettings.CoursePar, 0);
                            currentValidHandicap = Math.Max(0, Math.Min(36, currentValidHandicap));
                        }
                        else
                        {
                            if (scoresForCalculation.Count >= 3) // Need at least 3 scores for WHS calculation
                            {
                                currentValidHandicap = CalculateHandicapIndex(scoresForCalculation);
                            }
                        }
                    }
                }
                else
                {
                    // Week doesn't count for handicap - use the LAST VALID handicap converted to equivalent score
                    var equivalentScore = (int)(currentValidHandicap + leagueSettings.CoursePar);
                    scoresForCalculation.Add((equivalentScore, (int)leagueSettings.CourseRating, leagueSettings.SlopeRating));
                }
            }

            if (!scoresForCalculation.Any())
            {
                return await _playerSeasonStatsService.GetInitialHandicapAsync(playerId, seasonId); // No weeks played, return initial handicap
            }

            // Final handicap calculation with all scores (actual + previous handicap equivalents)
            decimal calculatedHandicap;

            if (leagueSettings.HandicapMethod == HandicapCalculationMethod.SimpleAverage)
            {
                // Simple Average Method: Handicap = Average Score - Course Par
                var averageScore = (decimal)scoresForCalculation.Average(s => s.score);
                calculatedHandicap = Math.Round(averageScore - leagueSettings.CoursePar, 0); // Round to whole numbers
                calculatedHandicap = Math.Max(0, Math.Min(36, calculatedHandicap)); // Cap between 0 and 36
            }
            else
            {
                // World Handicap System Method - limit to maxRounds for WHS calculation
                var recentScores = scoresForCalculation.TakeLast(maxRounds).ToList();
                calculatedHandicap = CalculateHandicapIndex(recentScores);
            }

            return calculatedHandicap;
        }

        /// <summary>
        /// Set session initial handicaps for all players in a season for a specific session start week.
        /// This is a convenience method to bulk-set session handicaps.
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <param name="sessionStartWeekNumber">The session start week number</param>
        /// <param name="defaultSessionHandicap">The default initial handicap to use for all players</param>
        /// <returns>Number of players updated</returns>
        public async Task<int> SetSessionHandicapsForAllPlayersAsync(Guid seasonId, int sessionStartWeekNumber, decimal defaultSessionHandicap)
        {
            // Verify the session start week exists
            var sessionStartWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.WeekNumber == sessionStartWeekNumber &&
                           w.SessionStart)
                .FirstOrDefaultAsync();

            if (sessionStartWeek == null)
            {
                throw new ArgumentException($"Week {sessionStartWeekNumber} is not a valid session start week for season {seasonId}");
            }

            // Get all players who have played in this season
            var seasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring && w.CountsForHandicap)
                .Select(w => w.Id)
                .ToListAsync();

            var playersInSeason = await _context.Matchups
                .Where(m => seasonWeeks.Contains(m.WeekId) &&
                           (m.PlayerAScore.HasValue || m.PlayerBScore.HasValue))
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToListAsync();

            int updatedCount = 0;

            foreach (var playerId in playersInSeason)
            {
                // Check if session handicap already exists
                var existingSessionHandicap = await _context.PlayerSessionHandicaps
                    .Where(psh => psh.PlayerId == playerId &&
                                 psh.SeasonId == seasonId &&
                                 psh.SessionStartWeekNumber == sessionStartWeekNumber)
                    .FirstOrDefaultAsync();

                if (existingSessionHandicap == null)
                {
                    // Create new session handicap
                    var newSessionHandicap = new PlayerSessionHandicap
                    {
                        PlayerId = playerId,
                        SeasonId = seasonId,
                        SessionStartWeekNumber = sessionStartWeekNumber,
                        SessionInitialHandicap = defaultSessionHandicap,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.PlayerSessionHandicaps.Add(newSessionHandicap);
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return updatedCount;
        }

        /// <summary>
        /// Set or update a session handicap for a specific player
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID</param>
        /// <param name="sessionStartWeekNumber">The session start week number</param>
        /// <param name="sessionHandicap">The session handicap value</param>
        /// <returns>True if created, false if updated</returns>
        public async Task<bool> SetPlayerSessionHandicapAsync(Guid playerId, Guid seasonId, int sessionStartWeekNumber, decimal sessionHandicap)
        {
            // Verify the session start week exists
            var sessionStartWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.WeekNumber == sessionStartWeekNumber &&
                           w.SessionStart)
                .FirstOrDefaultAsync();

            if (sessionStartWeek == null)
            {
                throw new ArgumentException($"Week {sessionStartWeekNumber} is not a valid session start week for season {seasonId}");
            }

            // Check if session handicap already exists
            var existingSessionHandicap = await _context.PlayerSessionHandicaps
                .Where(psh => psh.PlayerId == playerId &&
                             psh.SeasonId == seasonId &&
                             psh.SessionStartWeekNumber == sessionStartWeekNumber)
                .FirstOrDefaultAsync();

            if (existingSessionHandicap == null)
            {
                // Create new session handicap
                var newSessionHandicap = new PlayerSessionHandicap
                {
                    PlayerId = playerId,
                    SeasonId = seasonId,
                    SessionStartWeekNumber = sessionStartWeekNumber,
                    SessionInitialHandicap = sessionHandicap,
                    CreatedDate = DateTime.UtcNow
                };

                _context.PlayerSessionHandicaps.Add(newSessionHandicap);
                await _context.SaveChangesAsync();
                return true;
            }
            else
            {
                // Update existing session handicap
                existingSessionHandicap.SessionInitialHandicap = sessionHandicap;
                existingSessionHandicap.ModifiedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync();
                return false;
            }
        }

        /// <summary>
        /// Get all session handicaps for a player in a season
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID</param>
        /// <returns>List of session handicaps</returns>
        public async Task<List<PlayerSessionHandicap>> GetPlayerSessionHandicapsAsync(Guid playerId, Guid seasonId)
        {
            return await _context.PlayerSessionHandicaps
                .Where(psh => psh.PlayerId == playerId && psh.SeasonId == seasonId)
                .OrderBy(psh => psh.SessionStartWeekNumber)
                .ToListAsync();
        }

        /// <summary>
        /// Calculate and update a player's current handicap based on recent scores using configured method
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID to get league settings from</param>
        /// <param name="maxRounds">Maximum number of recent rounds to consider (default 20)</param>
        /// <returns>The updated handicap index</returns>
        public async Task<decimal> CalculateAndUpdateCurrentHandicapAsync(Guid playerId, Guid seasonId, int maxRounds = 20)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} not found");

            // Get league settings to determine calculation method
            var leagueSettings = await _leagueSettingsService.GetLeagueSettingsAsync(seasonId);

            // Get recent scores using the same methodology as GetPlayerHandicapUpToWeekAsync
            // This ensures consistency across all handicap calculations
            var recentScores = await GetRecentPlayerScoresForSeasonAsync(playerId, seasonId, maxRounds);

            if (!recentScores.Any())
            {
                return await _playerSeasonStatsService.GetInitialHandicapAsync(playerId, seasonId); // No scores available, return initial handicap
            }

            decimal newHandicap;

            if (leagueSettings.HandicapMethod == HandicapCalculationMethod.SimpleAverage)
            {
                // Simple Average Method: Handicap = Average Score - Course Par
                var averageScore = (decimal)recentScores.Average(s => s.score);
                newHandicap = Math.Round(averageScore - leagueSettings.CoursePar, 0); // Round to whole numbers
                newHandicap = Math.Max(0, Math.Min(36, newHandicap)); // Cap between 0 and 36
            }
            else
            {
                // World Handicap System Method
                // Override the scores with league settings for course rating and slope
                var adjustedScores = recentScores.Select(s => (s.score, (int)leagueSettings.CourseRating, leagueSettings.SlopeRating)).ToList();
                newHandicap = CalculateHandicapIndex(adjustedScores);
            }

            // Don't store calculated handicap - always calculate on demand
            return newHandicap;
        }

        /// <summary>
        /// Calculate and update a player's current handicap based on recent scores using WHS principles (legacy method)
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="maxRounds">Maximum number of recent rounds to consider (default 20)</param>
        /// <returns>The updated handicap index</returns>
        [Obsolete("Use CalculateAndUpdateCurrentHandicapAsync(playerId, seasonId, maxRounds) instead")]
        public async Task<decimal> CalculateAndUpdateCurrentHandicapAsync(Guid playerId, int maxRounds = 20)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} not found");

            // Get recent scores from matchups
            var recentScores = await GetRecentPlayerScoresAsync(playerId, maxRounds);

            if (!recentScores.Any())
            {
                return player.InitialHandicap; // No scores available, return initial handicap
            }

            // Calculate new handicap using WHS rules
            var newHandicap = CalculateHandicapIndex(recentScores);

            // Don't store calculated handicap - always calculate on demand
            return newHandicap;
        }

        /// <summary>
        /// Get player scores for handicap calculation starting from week 1
        /// </summary>
        private async Task<List<(int score, int courseRating, decimal slopeRating)>> GetRecentPlayerScoresAsync(Guid playerId, int maxRounds)
        {
            // Get all weeks that count for handicap calculations
            var weekIds = await _context.Weeks
                .Where(w => w.CountsForScoring &&
                           w.CountsForHandicap &&
                           !w.SpecialPointsAwarded.HasValue) // Exclude weeks with special points
                .Select(w => w.Id)
                .ToListAsync();

            // Get all non-absent scores for this player (same pattern as AverageScoreService)
            var playerScores = await _context.Matchups
                .Where(m => weekIds.Contains(m.WeekId) &&
                           (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                           ((m.PlayerAId == playerId && m.PlayerAScore.HasValue && !m.PlayerAAbsent) ||
                            (m.PlayerBId == playerId && m.PlayerBScore.HasValue && !m.PlayerBAbsent)))
                .Include(m => m.Week)
                .OrderByDescending(m => m.Week!.WeekNumber)
                .Take(maxRounds)
                .Select(m => m.PlayerAId == playerId ? m.PlayerAScore!.Value : m.PlayerBScore!.Value)
                .ToListAsync();

            return playerScores.Select(score => (score, 35, 113m)).ToList(); // 9-hole rating for Allentown Municipal
        }

        /// <summary>
        /// Get player scores for handicap calculation for a specific season using the same methodology
        /// as GetPlayerHandicapUpToWeekAsync (includes non-counting weeks with previous averages)
        /// </summary>
        private async Task<List<(int score, int courseRating, decimal slopeRating)>> GetRecentPlayerScoresForSeasonAsync(Guid playerId, Guid seasonId, int maxRounds)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null) return new List<(int, int, decimal)>();

            var leagueSettings = await _leagueSettingsService.GetLeagueSettingsAsync(seasonId);

            // Get ALL weeks in the season (both counting and non-counting)
            var allSeasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.CountsForScoring &&
                           !w.SpecialPointsAwarded.HasValue) // Exclude weeks with special points
                .OrderBy(w => w.WeekNumber)
                .ToListAsync();

            // Build scores array using previous valid handicap for non-counting weeks
            var scoresForCalculation = new List<(int score, int courseRating, decimal slopeRating)>();
            var currentValidHandicap = await _playerSeasonStatsService.GetInitialHandicapAsync(playerId, seasonId); // This only updates when we have a counting week with actual score

            foreach (var week in allSeasonWeeks)
            {
                if (week.CountsForHandicap)
                {
                    // Get actual score for this week
                    var actualScore = await _context.Matchups
                        .Where(m => m.WeekId == week.Id &&
                                   (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                                   ((m.PlayerAId == playerId && m.PlayerAScore.HasValue && !m.PlayerAAbsent) ||
                                    (m.PlayerBId == playerId && m.PlayerBScore.HasValue && !m.PlayerBAbsent)))
                        .Select(m => m.PlayerAId == playerId ? m.PlayerAScore!.Value : m.PlayerBScore!.Value)
                        .FirstOrDefaultAsync();

                    if (actualScore > 0)
                    {
                        scoresForCalculation.Add((actualScore, (int)leagueSettings.CourseRating, leagueSettings.SlopeRating));

                        // Update the valid handicap AFTER adding this score for future non-counting weeks
                        if (leagueSettings.HandicapMethod == HandicapCalculationMethod.SimpleAverage)
                        {
                            var avgScore = (decimal)scoresForCalculation.Average(s => s.score);
                            currentValidHandicap = Math.Round(avgScore - leagueSettings.CoursePar, 0);
                            currentValidHandicap = Math.Max(0, Math.Min(36, currentValidHandicap));
                        }
                        else
                        {
                            if (scoresForCalculation.Count >= 3) // Need at least 3 scores for calculation
                            {
                                currentValidHandicap = CalculateHandicapIndex(scoresForCalculation);
                            }
                        }
                    }
                }
                else
                {
                    // Week doesn't count for handicap - use the LAST VALID handicap converted to equivalent score
                    var equivalentScore = (int)(currentValidHandicap + leagueSettings.CoursePar);
                    scoresForCalculation.Add((equivalentScore, (int)leagueSettings.CourseRating, leagueSettings.SlopeRating));
                }
            }

            // Return most recent scores up to maxRounds
            return scoresForCalculation.TakeLast(maxRounds).ToList();
        }

        /// <summary>
        /// Calculate handicap index using World Handicap System principles
        /// </summary>
        private decimal CalculateHandicapIndex(List<(int score, int courseRating, decimal slopeRating)> rounds)
        {
            if (!rounds.Any()) return 0;

            // Calculate score differentials
            var differentials = rounds
                .Select(round => (decimal)(round.score - round.courseRating) * 113m / round.slopeRating)
                .OrderBy(d => d)
                .ToList();

            // Get number of differentials to use based on WHS rules
            int differentialsToUse = GetDifferentialsCount(differentials.Count);

            if (differentialsToUse == 0) return 0;

            // Average the lowest differentials and multiply by 0.96
            var selectedDifferentials = differentials.Take(differentialsToUse);
            var averageDifferential = selectedDifferentials.Average();
            var handicapIndex = averageDifferential * 0.96m;

            return Math.Max(0, Math.Min(36, Math.Round(handicapIndex, 0))); // Round to whole numbers
        }

        /// <summary>
        /// Get number of differentials to use based on total available (WHS rules)
        /// </summary>
        private int GetDifferentialsCount(int totalDifferentials)
        {
            return totalDifferentials switch
            {
                >= 20 => 8,
                19 => 7,
                >= 17 => 6,
                >= 15 => 5,
                >= 12 => 4,
                >= 9 => 3,
                >= 6 => 2,
                >= 3 => 1,
                _ => 0
            };
        }

        /// <summary>
        /// Suggest session handicaps based on recent performance
        /// Call this at the start of each session to get recommended session handicaps
        /// </summary>
        public async Task<Dictionary<Guid, decimal>> GetSuggestedSessionHandicapsAsync(Guid seasonId, int sessionStartWeekNumber)
        {
            var suggestions = new Dictionary<Guid, decimal>();

            // Get all players who have played in this season
            var seasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring && w.CountsForHandicap)
                .Select(w => w.Id)
                .ToListAsync();

            var playersInSeason = await _context.Matchups
                .Where(m => seasonWeeks.Contains(m.WeekId) &&
                           (m.PlayerAScore.HasValue || m.PlayerBScore.HasValue))
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToListAsync();

            foreach (var playerId in playersInSeason)
            {
                var currentHandicap = await CalculateAndUpdateCurrentHandicapAsync(playerId, seasonId);
                suggestions[playerId] = currentHandicap;
            }

            return suggestions;
        }

        /// <summary>
        /// Bulk calculate and update handicaps for all players in a season using league settings
        /// </summary>
        /// <param name="seasonId">Season ID</param>
        /// <param name="maxRounds">Maximum number of rounds to consider per player</param>
        /// <returns>Dictionary of player ID to calculated handicap</returns>
        public async Task<Dictionary<Guid, decimal>> BulkCalculateSeasonHandicapsAsync(Guid seasonId, int maxRounds = 20)
        {
            var results = new Dictionary<Guid, decimal>();

            // Get league settings for this season
            var leagueSettings = await _leagueSettingsService.GetLeagueSettingsAsync(seasonId);

            // Get all players who have played in this season
            var seasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring && w.CountsForHandicap)
                .Select(w => w.Id)
                .ToListAsync();

            var playersInSeason = await _context.Matchups
                .Where(m => seasonWeeks.Contains(m.WeekId) &&
                           (m.PlayerAScore.HasValue || m.PlayerBScore.HasValue))
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToListAsync();

            foreach (var playerId in playersInSeason)
            {
                try
                {
                    var handicap = await CalculateAndUpdateCurrentHandicapAsync(playerId, seasonId, maxRounds);
                    results[playerId] = handicap;
                }
                catch (Exception ex)
                {
                    // Log error but continue with other players
                    Console.WriteLine($"Error calculating handicap for player {playerId}: {ex.Message}");
                }
            }

            return results;
        }

        /// <summary>
        /// Calculate handicaps for all players in a season based on their scores (legacy method)
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <param name="courseRating">9-hole course rating (default 35)</param>
        /// <param name="slopeRating">Slope rating (default 113)</param>
        /// <returns>Dictionary of player IDs and their calculated handicaps</returns>
        [Obsolete("Use BulkCalculateSeasonHandicapsAsync instead for season-specific settings")]
        public async Task<Dictionary<Guid, decimal>> CalculateAllPlayerHandicapsAsync(Guid seasonId, int courseRating = 35, decimal slopeRating = 113)
        {
            var results = new Dictionary<Guid, decimal>();

            // Get all players who have played in this season
            var seasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring && w.CountsForHandicap)
                .Select(w => w.Id)
                .ToListAsync();

            var playersInSeason = await _context.Matchups
                .Where(m => seasonWeeks.Contains(m.WeekId) &&
                           (m.PlayerAScore.HasValue || m.PlayerBScore.HasValue))
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToListAsync();

            foreach (var playerId in playersInSeason)
            {
                try
                {
                    // Get player's scores
                    var scores = await GetRecentPlayerScoresAsync(playerId, 20);

                    if (scores.Any())
                    {
                        // Override course parameters if specified
                        var adjustedScores = scores.Select(s => (s.score, courseRating, slopeRating)).ToList();
                        var handicap = CalculateHandicapIndex(adjustedScores);
                        results[playerId] = handicap;

                        // Don't update CurrentHandicap as it should always be calculated on demand
                    }
                }
                catch (Exception ex)
                {
                    // Log error but continue with other players
                    Console.WriteLine($"Error calculating handicap for player {playerId}: {ex.Message}");
                }
            }

            await _context.SaveChangesAsync();
            return results;
        }

        /// <summary>
        /// Calculate handicap for a single player with custom course parameters
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="courseRating">9-hole course rating</param>
        /// <param name="slopeRating">Slope rating</param>
        /// <param name="maxRounds">Maximum number of recent rounds to consider</param>
        /// <returns>Calculated handicap index</returns>
        public async Task<decimal> CalculatePlayerHandicapAsync(Guid playerId, int courseRating = 35, decimal slopeRating = 113, int maxRounds = 20)
        {
            var scores = await GetRecentPlayerScoresAsync(playerId, maxRounds);

            if (!scores.Any())
                return 0;

            // Override course parameters
            var adjustedScores = scores.Select(s => (s.score, courseRating, slopeRating)).ToList();
            return CalculateHandicapIndex(adjustedScores);
        }

    }
}

// Note: This service now uses the same efficient pattern as AverageScoreService,
// querying Matchups directly to get player scores instead of complex HoleScores aggregation.
// Scores are retrieved from the PlayerAScore/PlayerBScore fields in the Matchups table.
