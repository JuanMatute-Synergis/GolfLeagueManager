using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class AverageScoreService
    {
        private readonly AppDbContext _context;

        public AverageScoreService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Calculate and update the current average score for a player after a new score is entered.
        /// The calculation includes the player's session-specific initial average score as a baseline, 
        /// then incorporates actual scores from all weeks in the current session up to and including the specified week.
        /// If no session-specific initial average is set, falls back to the player's regular initial average.
        /// Formula: (Session Initial Average + Sum of actual scores) / (1 + Number of actual rounds played)
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

            // Find the most recent SessionStart week up to and including the current week
            var sessionStartWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber <= upToWeekNumber && w.SessionStart)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefaultAsync();
            
            // If no session start found, use week 1 as session start
            int sessionStartWeekNumber = sessionStartWeek?.WeekNumber ?? 1;

            // Get the session-specific initial average or fall back to player's initial average
            var sessionAverage = await _context.PlayerSessionAverages
                .Where(psa => psa.PlayerId == playerId && 
                             psa.SeasonId == seasonId && 
                             psa.SessionStartWeekNumber == sessionStartWeekNumber)
                .FirstOrDefaultAsync();
            
            decimal initialAverageForSession = sessionAverage?.SessionInitialAverage ?? player.InitialAverageScore;

            // Get all weeks before the session start (these are represented by the session initial average)
            var weeksBeforeSession = await _context.Weeks
                .CountAsync(w => w.SeasonId == seasonId && 
                               w.CountsForScoring && 
                               w.WeekNumber < sessionStartWeekNumber);

            // Get all weeks in the current session up to and including the specified week
            var sessionWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && 
                           w.CountsForScoring && 
                           w.WeekNumber >= sessionStartWeekNumber && 
                           w.WeekNumber <= upToWeekNumber)
                .ToListAsync();

            var weekIds = sessionWeeks.Select(w => w.Id).ToList();

            // Get all non-absent scores for this player in these weeks
            var playerScores = await _context.Matchups
                .Where(m => weekIds.Contains(m.WeekId) &&
                           (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                           ((m.PlayerAId == playerId && m.PlayerAScore.HasValue && !m.PlayerAAbsent) ||
                            (m.PlayerBId == playerId && m.PlayerBScore.HasValue && !m.PlayerBAbsent)))
                .Select(m => new
                {
                    WeekId = m.WeekId,
                    Score = m.PlayerAId == playerId ? m.PlayerAScore : m.PlayerBScore
                })
                .Where(s => s.Score.HasValue)
                .ToListAsync();

            // Calculate average correctly: session initial average represents ALL weeks before the session start
            // Formula: (Session Initial Average × Weeks before session + Sum of actual scores in session) / (Weeks before session + Actual rounds in session)
            var totalScore = (initialAverageForSession * weeksBeforeSession) + playerScores.Sum(s => s.Score!.Value);
            var totalRounds = weeksBeforeSession + playerScores.Count;
            var averageScore = totalRounds > 0 ? totalScore / totalRounds : initialAverageForSession;
            player.CurrentAverageScore = Math.Round(averageScore, 2);

            await _context.SaveChangesAsync();
            return player.CurrentAverageScore;
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
            // Get the latest week number for this season
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

            // Get all weeks for the season that count for scoring
            var seasonWeeks = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring)
                .ToListAsync();

            var weekIds = seasonWeeks.Select(w => w.Id).ToList();

            // Get all non-absent scores for this player in these weeks
            var playerScores = await _context.Matchups
                .Where(m => weekIds.Contains(m.WeekId) &&
                           (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                           ((m.PlayerAId == playerId && m.PlayerAScore.HasValue && !m.PlayerAAbsent) ||
                            (m.PlayerBId == playerId && m.PlayerBScore.HasValue && !m.PlayerBAbsent)))
                .Select(m => new
                {
                    WeekId = m.WeekId,
                    Score = m.PlayerAId == playerId ? m.PlayerAScore : m.PlayerBScore
                })
                .Where(s => s.Score.HasValue)
                .ToListAsync();

            var scores = playerScores.Select(s => s.Score!.Value).ToList();

            // Calculate average including initial average score as baseline
            var averageScoreCalculated = scores.Count > 0 
                ? Math.Round(((decimal)player.InitialAverageScore + scores.Sum()) / (1 + scores.Count), 2)
                : player.InitialAverageScore;

            return new PlayerScoringStats
            {
                PlayerId = playerId,
                PlayerName = $"{player.FirstName} {player.LastName}",
                InitialAverageScore = player.InitialAverageScore,
                CurrentAverageScore = player.CurrentAverageScore,
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
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring)
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
        /// Calculate a player's average score up to (but not including) a given week, using only non-absent rounds
        /// from the same session. The calculation includes the player's session-specific initial average score as a baseline, 
        /// then incorporates actual scores from previous weeks in the same session.
        /// If no session-specific initial average is set, falls back to the player's regular initial average.
        /// Formula: (Session Initial Average + Sum of actual scores) / (1 + Number of actual rounds played)
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID</param>
        /// <param name="upToWeekNumber">The week number (exclusive)</param>
        /// <returns>The calculated average score including session baseline</returns>
        public async Task<decimal> GetPlayerAverageScoreUpToWeekAsync(Guid playerId, Guid seasonId, int upToWeekNumber)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} not found");

            // Find the most recent SessionStart week up to but not including the current week
            var sessionStartWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber < upToWeekNumber && w.SessionStart)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefaultAsync();
            
            // If no session start found before current week, use week 1 as session start
            int sessionStartWeekNumber = sessionStartWeek?.WeekNumber ?? 1;

            // Get the session-specific initial average or fall back to player's initial average
            var sessionAverage = await _context.PlayerSessionAverages
                .Where(psa => psa.PlayerId == playerId && 
                             psa.SeasonId == seasonId && 
                             psa.SessionStartWeekNumber == sessionStartWeekNumber)
                .FirstOrDefaultAsync();
            
            decimal initialAverageForSession = sessionAverage?.SessionInitialAverage ?? player.InitialAverageScore;

            // Get all weeks before the session start (these are represented by the session initial average)
            var weeksBeforeSession = await _context.Weeks
                .CountAsync(w => w.SeasonId == seasonId && 
                               w.CountsForScoring && 
                               w.WeekNumber < sessionStartWeekNumber);

            // Get all weeks in the current session that are before the given week
            var priorWeeksInSession = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && 
                           w.CountsForScoring && 
                           w.WeekNumber >= sessionStartWeekNumber && 
                           w.WeekNumber < upToWeekNumber)
                .Select(w => w.Id)
                .ToListAsync();

            // Get all non-absent scores for this player in these weeks
            var playerScores = await _context.Matchups
                .Where(m => priorWeeksInSession.Contains(m.WeekId) &&
                           (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                           ((m.PlayerAId == playerId && m.PlayerAScore.HasValue && !m.PlayerAAbsent) ||
                            (m.PlayerBId == playerId && m.PlayerBScore.HasValue && !m.PlayerBAbsent)))
                .Select(m => m.PlayerAId == playerId ? m.PlayerAScore : m.PlayerBScore)
                .Where(score => score.HasValue)
                .ToListAsync();

            // Calculate average correctly: session initial average represents ALL weeks before the session start
            // Formula: (Session Initial Average × Weeks before session + Sum of actual scores in session) / (Weeks before session + Actual rounds in session)
            var totalScore = (initialAverageForSession * weeksBeforeSession) + playerScores.Sum(s => s!.Value);
            var totalRounds = weeksBeforeSession + playerScores.Count;
            var averageScore = totalRounds > 0 ? totalScore / totalRounds : initialAverageForSession;
            return Math.Round(averageScore, 2);
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
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring)
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
