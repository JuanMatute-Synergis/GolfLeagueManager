using Microsoft.EntityFrameworkCore;
using GolfLeagueManager.Business;

namespace GolfLeagueManager
{
    public class AverageScoreService
    {
        private readonly AppDbContext _context;
        private readonly PlayerSeasonStatsService _playerSeasonStatsService;
        private readonly LeagueSettingsService _leagueSettingsService;

        public AverageScoreService(AppDbContext context, PlayerSeasonStatsService playerSeasonStatsService, LeagueSettingsService leagueSettingsService)
        {
            _context = context;
            _playerSeasonStatsService = playerSeasonStatsService;
            _leagueSettingsService = leagueSettingsService;
        }

        /// <summary>
        /// Calculate and update the current average score for a player after a new score is entered.
        /// The calculation method depends on the league settings (SimpleAverage or LegacyWeightedAverage).
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID to calculate average for</param>
        /// <param name="upToWeekNumber">The week number to calculate up to (inclusive)</param>
        /// <returns>The updated current average score</returns>
        public async Task<decimal> UpdatePlayerAverageScoreAsync(Guid playerId, Guid seasonId, int upToWeekNumber)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                throw new ArgumentException($"Player with ID {playerId} not found");
            }

            // Get league settings to determine calculation method
            var leagueSettings = await _leagueSettingsService.GetLeagueSettingsAsync(seasonId);

            if (leagueSettings.AverageMethod == AverageCalculationMethod.LegacyWeightedAverage)
            {
                return await CalculateLegacyWeightedAverageAsync(playerId, seasonId, upToWeekNumber, leagueSettings.LegacyInitialWeight);
            }
            else
            {
                return await CalculateSimpleAverageAsync(playerId, seasonId, upToWeekNumber);
            }
        }

        /// <summary>
        /// Calculate average using the current simple average method
        /// Formula: (Initial Average + Sum of actual scores) / (1 + Number of actual rounds played)
        /// </summary>
        private async Task<decimal> CalculateSimpleAverageAsync(Guid playerId, Guid seasonId, int upToWeekNumber)
        {
            // Get the player's initial average score for this season
            decimal initialAverage = await _playerSeasonStatsService.GetInitialAverageScoreAsync(playerId, seasonId);

            // Get ALL weeks in the season up to and including the specified week
            var allSeasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.CountsForScoring &&
                           w.WeekNumber <= upToWeekNumber)
                .OrderBy(w => w.WeekNumber)
                .ToListAsync();

            // Collect only actual scores from weeks that count for handicap
            var actualScores = new List<decimal>();

            foreach (var week in allSeasonWeeks)
            {
                if (week.CountsForHandicap)
                {
                    // Get actual score for this week from HoleScores table via Matchup
                    var actualScore = await _context.HoleScores
                        .Join(_context.Matchups,
                            hs => hs.MatchupId,
                            m => m.Id,
                            (hs, m) => new { HoleScore = hs, Matchup = m })
                        .Where(x => x.Matchup.WeekId == week.Id &&
                                   (x.Matchup.PlayerAId == playerId || x.Matchup.PlayerBId == playerId))
                        .SumAsync(x => x.Matchup.PlayerAId == playerId ?
                                      (x.HoleScore.PlayerAScore ?? 0) :
                                      (x.HoleScore.PlayerBScore ?? 0));

                    if (actualScore > 0)
                    {
                        actualScores.Add(actualScore);
                    }
                }
                // Weeks that don't count for handicap are ignored in SimpleAverage method
            }

            // Simple average formula: (initial + sum of actual scores) / (1 + count of actual scores)
            var totalScore = initialAverage + actualScores.Sum();
            var totalCount = 1 + actualScores.Count;
            var averageScore = totalScore / totalCount;

            return Math.Round(averageScore, 2, MidpointRounding.AwayFromZero);
        }

        /// <summary>
        /// Calculate average using the legacy weighted average method
        /// Formula: (initial_average + sum_of_scores + sum_of_phantom_scores) / total_weeks
        /// For non-counting weeks, use the current running average as phantom score
        /// </summary>
        private async Task<decimal> CalculateLegacyWeightedAverageAsync(Guid playerId, Guid seasonId, int upToWeekNumber, int initialWeight)
        {
            // Get the player's initial average score for this season
            decimal initialAverage = await _playerSeasonStatsService.GetInitialAverageScoreAsync(playerId, seasonId);

            // Get ALL weeks in the season up to and including the specified week
            var allSeasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId &&
                           w.CountsForScoring &&
                           w.WeekNumber <= upToWeekNumber)
                .OrderBy(w => w.WeekNumber)
                .ToListAsync();

            // Start with initial average counting as 1 week
            decimal totalScore = initialAverage;
            int totalWeeks = 1;
            decimal currentRunningAverage = Math.Round(initialAverage, 2, MidpointRounding.AwayFromZero);

            //get the player's name
            var playerName = await _context.Players
                .Where(p => p.Id == playerId)
                .Select(p => p.FirstName)
                .FirstOrDefaultAsync();

            if (playerName == "Ray")
            {
                Console.WriteLine($"Player Name: {playerName}");
            }

            foreach (var week in allSeasonWeeks)
            {
                // Increment total weeks for both counting and non-counting weeks
                totalWeeks++;

                if (week.CountsForHandicap)
                {
                    // Get actual score for this week from HoleScores table via Matchup
                    var actualScore = await _context.HoleScores
                        .Join(_context.Matchups,
                            hs => hs.MatchupId,
                            m => m.Id,
                            (hs, m) => new { HoleScore = hs, Matchup = m })
                        .Where(x => x.Matchup.WeekId == week.Id &&
                                   (x.Matchup.PlayerAId == playerId || x.Matchup.PlayerBId == playerId))
                        .SumAsync(x => x.Matchup.PlayerAId == playerId ?
                                      (x.HoleScore.PlayerAScore ?? 0) :
                                      (x.HoleScore.PlayerBScore ?? 0));

                    if (actualScore > 0)
                    {
                        // Add actual score to total
                        totalScore += actualScore;
                        //dont count this week as phantom
                        currentRunningAverage = Math.Round(totalScore / totalWeeks, 2, MidpointRounding.AwayFromZero);
                    }
                    else
                    {
                        totalWeeks--;
                        //dont count this week as phantom, no operation
                        continue;
                    }
                }
                else
                {
                    // Week doesn't count for handicap (like weeks 1-3), use current running average
                    totalScore += Math.Round(currentRunningAverage, 2, MidpointRounding.AwayFromZero);
                    totalScore = Math.Round(totalScore, 2, MidpointRounding.AwayFromZero);
                }

                // Update the current running average for the next iteration
                currentRunningAverage = Math.Round(totalScore / totalWeeks, 2, MidpointRounding.AwayFromZero);
            }

            // Calculate final weighted average
            if (totalWeeks == 0) return Math.Round(initialAverage, 2, MidpointRounding.AwayFromZero);
            decimal weightedAverage = Math.Round(totalScore / totalWeeks, 2, MidpointRounding.AwayFromZero);
            return weightedAverage;
        }

        /// <summary>
        /// Calculate and update the current average score for a player after a new score is entered.
        /// This overload calculates the average using all weeks in the season (for backward compatibility).
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID to calculate average for</param>
        /// <returns>The updated current average score</returns>
        public async Task<decimal> UpdatePlayerAverageScoreAsync(Guid playerId, Guid seasonId)
        {
            // Get the latest week number for this season (include all weeks, not just counting ones)
            var latestWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefaultAsync();

            var upToWeekNumber = latestWeek?.WeekNumber ?? 1;
            return await UpdatePlayerAverageScoreAsync(playerId, seasonId, upToWeekNumber);
        }

        /// <summary>
        /// Update average scores for all players in a specific week after scores are entered
        /// </summary>
        /// <param name="weekId">The week ID</param>
        /// <returns>Dictionary of player IDs and their updated average scores</returns>
        public async Task<Dictionary<Guid, decimal>> UpdateAverageScoresForWeekAsync(Guid weekId)
        {
            var week = await _context.Weeks.FindAsync(weekId);
            if (week == null)
            {
                throw new ArgumentException($"Week with ID {weekId} not found");
            }

            // Get all matchups for this week that have scores
            var weekMatchups = await _context.Matchups
                .Where(m => m.WeekId == weekId &&
                           (m.PlayerAScore.HasValue || m.PlayerBScore.HasValue))
                .ToListAsync();

            var results = new Dictionary<Guid, decimal>();
            var playersToUpdate = new HashSet<Guid>();

            // Collect all players who have scores in this week
            foreach (var matchup in weekMatchups)
            {
                if (matchup.PlayerAScore.HasValue)
                    playersToUpdate.Add(matchup.PlayerAId);
                if (matchup.PlayerBScore.HasValue)
                    playersToUpdate.Add(matchup.PlayerBId);
            }

            // Update average score for each player
            foreach (var playerId in playersToUpdate)
            {
                var updatedAverage = await UpdatePlayerAverageScoreAsync(playerId, week.SeasonId, week.WeekNumber);
                results[playerId] = updatedAverage;
            }

            return results;
        }

        /// <summary>
        /// Get scoring statistics for a player in a season
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID</param>
        /// <returns>Player scoring statistics</returns>
        public async Task<PlayerScoringStats> GetPlayerScoringStatsAsync(Guid playerId, Guid seasonId)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                throw new ArgumentException($"Player with ID {playerId} not found");
            }

            // Get all weeks for the season that count for scoring and handicap
            var seasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring && w.CountsForHandicap)
                .ToListAsync();

            var weekIds = seasonWeeks.Select(w => w.Id).ToList();

            // Get all non-absent scores for this player in these weeks from HoleScores
            var playerScores = await _context.HoleScores
                .Join(_context.Matchups,
                    hs => hs.MatchupId,
                    m => m.Id,
                    (hs, m) => new { HoleScore = hs, Matchup = m })
                .Where(x => weekIds.Contains(x.Matchup.WeekId) &&
                           (x.Matchup.PlayerAId == playerId || x.Matchup.PlayerBId == playerId) &&
                           ((x.Matchup.PlayerAId == playerId && !x.Matchup.PlayerAAbsent) ||
                            (x.Matchup.PlayerBId == playerId && !x.Matchup.PlayerBAbsent)))
                .GroupBy(x => x.Matchup.WeekId)
                .Select(g => new
                {
                    WeekId = g.Key,
                    Score = g.Sum(x => x.Matchup.PlayerAId == playerId ?
                                      (x.HoleScore.PlayerAScore ?? 0) :
                                      (x.HoleScore.PlayerBScore ?? 0))
                })
                .Where(s => s.Score > 0)
                .ToListAsync();

            var scores = playerScores.Select(s => s.Score).ToList();

            // Get initial and current average scores for this season
            var initialAverageScore = await _playerSeasonStatsService.GetInitialAverageScoreAsync(playerId, seasonId);
            var currentAverageScore = await _playerSeasonStatsService.GetCurrentAverageScoreAsync(playerId, seasonId);

            // Calculate average including initial average score as baseline
            var averageScoreCalculated = scores.Count > 0
                ? Math.Round((initialAverageScore + scores.Sum()) / (1 + scores.Count), 2, MidpointRounding.AwayFromZero)
                : initialAverageScore;

            return new PlayerScoringStats
            {
                PlayerId = playerId,
                PlayerName = $"{player.FirstName} {player.LastName}",
                InitialAverageScore = initialAverageScore,
                CurrentAverageScore = currentAverageScore,
                RoundsPlayed = scores.Count,
                BestScore = scores.Count > 0 ? scores.Min() : null,
                WorstScore = scores.Count > 0 ? scores.Max() : null,
                TotalStrokes = scores.Count > 0 ? scores.Sum() : 0,
                AverageScoreCalculated = averageScoreCalculated
            };
        }

        /// <summary>
        /// Recalculate all player average scores for a season (useful for corrections or initial calculations)
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <returns>Dictionary of player IDs and their recalculated average scores</returns>
        public async Task<Dictionary<Guid, decimal>> RecalculateAllAverageScoresForSeasonAsync(Guid seasonId)
        {
            // Get all players who have scores in this season
            var seasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring && w.CountsForHandicap)
                .Select(w => w.Id)
                .ToListAsync();

            var playersInSeason = await _context.Matchups
                .Where(m => seasonWeeks.Contains(m.WeekId) &&
                           (m.PlayerAScore.HasValue || m.PlayerBScore.HasValue))
                .ToListAsync();

            var playerIds = playersInSeason
                .SelectMany(m => new[] { m.PlayerAId, m.PlayerBId })
                .Distinct()
                .ToList();

            var results = new Dictionary<Guid, decimal>();

            foreach (var playerId in playerIds)
            {
                var updatedAverage = await UpdatePlayerAverageScoreAsync(playerId, seasonId);
                results[playerId] = updatedAverage;
            }

            return results;
        }

        /// <summary>
        /// <summary>
        /// Calculate a player's average score up to (but not including) a given week, using the phantom score methodology.
        /// This method uses the same logic as UpdatePlayerAverageScoreAsync to ensure consistency.
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID</param>
        /// <param name="upToWeekNumber">The week number (exclusive)</param>
        /// <returns>The calculated average score using phantom score methodology</returns>
        public async Task<decimal> GetPlayerAverageScoreUpToWeekAsync(Guid playerId, Guid seasonId, int upToWeekNumber)
        {
            // Use the existing phantom score methodology by calling UpdatePlayerAverageScoreAsync
            // with upToWeekNumber to get the average up to and including the given week
            return await UpdatePlayerAverageScoreAsync(playerId, seasonId, upToWeekNumber);
        }

        /// <summary>
        /// Set session initial averages for all players in a season for a specific session start week.
        /// This is a convenience method to bulk-set session averages.
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <param name="sessionStartWeekNumber">The session start week number</param>
        /// <param name="defaultSessionAverage">The default initial average to use for all players</param>
        /// <returns>Number of players updated</returns>
        public async Task<int> SetSessionAveragesForAllPlayersAsync(Guid seasonId, int sessionStartWeekNumber, decimal defaultSessionAverage)
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
                // Check if session average already exists
                var existingSessionAverage = await _context.PlayerSessionAverages
                    .Where(psa => psa.PlayerId == playerId &&
                                 psa.SeasonId == seasonId &&
                                 psa.SessionStartWeekNumber == sessionStartWeekNumber)
                    .FirstOrDefaultAsync();

                if (existingSessionAverage == null)
                {
                    // Create new session average
                    var newSessionAverage = new PlayerSessionAverage
                    {
                        PlayerId = playerId,
                        SeasonId = seasonId,
                        SessionStartWeekNumber = sessionStartWeekNumber,
                        SessionInitialAverage = defaultSessionAverage,
                        CreatedDate = DateTime.UtcNow
                    };

                    _context.PlayerSessionAverages.Add(newSessionAverage);
                    updatedCount++;
                }
            }

            await _context.SaveChangesAsync();
            return updatedCount;
        }
    }

    /// <summary>
    /// Player scoring statistics model
    /// </summary>
    public class PlayerScoringStats
    {
        public Guid PlayerId { get; set; }
        public string PlayerName { get; set; } = string.Empty;
        public decimal InitialAverageScore { get; set; }
        public decimal CurrentAverageScore { get; set; }
        public int RoundsPlayed { get; set; }
        public int? BestScore { get; set; }
        public int? WorstScore { get; set; }
        public int TotalStrokes { get; set; }
        public decimal AverageScoreCalculated { get; set; }
    }
}
