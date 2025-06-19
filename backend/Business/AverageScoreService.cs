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
        /// Calculate and update the current average score for a player after a new score is entered
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID to calculate average for</param>
        /// <returns>The updated current average score</returns>
        public async Task<decimal> UpdatePlayerAverageScoreAsync(Guid playerId, Guid seasonId)
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

            // Get all scores for this player in these weeks
            var playerScores = await _context.Matchups
                .Where(m => weekIds.Contains(m.WeekId) &&
                           (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                           ((m.PlayerAId == playerId && m.PlayerAScore.HasValue) ||
                            (m.PlayerBId == playerId && m.PlayerBScore.HasValue)))
                .Select(m => new
                {
                    WeekId = m.WeekId,
                    Score = m.PlayerAId == playerId ? m.PlayerAScore : m.PlayerBScore
                })
                .Where(s => s.Score.HasValue)
                .ToListAsync();

            if (playerScores.Count == 0)
            {
                // No scores yet, keep current average as initial average
                player.CurrentAverageScore = player.InitialAverageScore;
            }
            else
            {
                // Calculate the average of all scores
                var totalScore = playerScores.Sum(s => s.Score!.Value);
                var averageScore = (decimal)totalScore / playerScores.Count;
                player.CurrentAverageScore = Math.Round(averageScore, 2);
            }

            await _context.SaveChangesAsync();
            return player.CurrentAverageScore;
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
                var updatedAverage = await UpdatePlayerAverageScoreAsync(playerId, week.SeasonId);
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

            // Get all scores for this player in these weeks
            var playerScores = await _context.Matchups
                .Where(m => weekIds.Contains(m.WeekId) &&
                           (m.PlayerAId == playerId || m.PlayerBId == playerId) &&
                           ((m.PlayerAId == playerId && m.PlayerAScore.HasValue) ||
                            (m.PlayerBId == playerId && m.PlayerBScore.HasValue)))
                .Select(m => new
                {
                    WeekId = m.WeekId,
                    Score = m.PlayerAId == playerId ? m.PlayerAScore : m.PlayerBScore
                })
                .Where(s => s.Score.HasValue)
                .ToListAsync();

            var scores = playerScores.Select(s => s.Score!.Value).ToList();

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
                AverageScoreCalculated = scores.Count > 0 ? Math.Round((decimal)scores.Average(), 2) : 0
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
