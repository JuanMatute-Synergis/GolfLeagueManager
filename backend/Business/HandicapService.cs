using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager
{
    public class HandicapService
    {
        private readonly AppDbContext _context;

        public HandicapService(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Get a player's session-specific handicap for calculations, using session context
        /// similar to how we handle session averages.
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="seasonId">The season ID</param>
        /// <param name="weekNumber">The week number to get handicap for</param>
        /// <returns>The session-specific handicap or current handicap if no session handicap is set</returns>
        public async Task<decimal> GetPlayerSessionHandicapAsync(Guid playerId, Guid seasonId, int weekNumber)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} not found");

            // Find the most recent SessionStart week up to and including the current week
            var sessionStartWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && w.WeekNumber <= weekNumber && w.SessionStart)
                .OrderByDescending(w => w.WeekNumber)
                .FirstOrDefaultAsync();
            
            // If no session start found, use week 1 as session start
            int sessionStartWeekNumber = sessionStartWeek?.WeekNumber ?? 1;

            // Get the session-specific initial handicap or fall back to player's current handicap
            var sessionHandicap = await _context.PlayerSessionHandicaps
                .Where(psh => psh.PlayerId == playerId && 
                             psh.SeasonId == seasonId && 
                             psh.SessionStartWeekNumber == sessionStartWeekNumber)
                .FirstOrDefaultAsync();
            
            return sessionHandicap?.SessionInitialHandicap ?? player.CurrentHandicap;
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
        /// Calculate and update a player's current handicap based on recent scores using WHS principles
        /// </summary>
        /// <param name="playerId">The player's ID</param>
        /// <param name="maxRounds">Maximum number of recent rounds to consider (default 20)</param>
        /// <returns>The updated handicap index</returns>
        public async Task<decimal> CalculateAndUpdateCurrentHandicapAsync(Guid playerId, int maxRounds = 20)
        {
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
                throw new ArgumentException($"Player with ID {playerId} not found");

            // Get recent scores from matchups
            var recentScores = await GetRecentPlayerScoresAsync(playerId, maxRounds);
            
            if (!recentScores.Any())
            {
                return player.CurrentHandicap; // No scores available, keep current handicap
            }

            // Calculate new handicap using WHS rules
            var newHandicap = CalculateHandicapIndex(recentScores);
            
            // Update player's current handicap
            player.CurrentHandicap = newHandicap;
            await _context.SaveChangesAsync();
            
            return newHandicap;
        }

        /// <summary>
        /// Get recent player scores for handicap calculation
        /// </summary>
        private async Task<List<(int score, int courseRating, decimal slopeRating)>> GetRecentPlayerScoresAsync(Guid playerId, int maxRounds)
        {
            var scores = await _context.Matchups
                .Where(m => (m.PlayerAId == playerId && m.PlayerAScore.HasValue) ||
                           (m.PlayerBId == playerId && m.PlayerBScore.HasValue))
                .Include(m => m.Week)
                .Where(m => m.Week != null && m.Week.CountsForScoring)
                .OrderByDescending(m => m.Week!.WeekNumber)
                .Take(maxRounds)
                .Select(m => new
                {
                    Score = m.PlayerAId == playerId ? m.PlayerAScore!.Value : m.PlayerBScore!.Value,
                    WeekNumber = m.Week!.WeekNumber,
                    CourseRating = 35, // 9-hole rating for Allentown Municipal (adjust as needed)
                    SlopeRating = 113m // Standard slope rating (adjust if you know the actual slope)
                })
                .ToListAsync();

            return scores.Select(s => (s.Score, s.CourseRating, s.SlopeRating)).ToList();
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

            return Math.Max(0, Math.Min(36, Math.Round(handicapIndex, 1)));
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
                .Where(w => w.SeasonId == seasonId && w.CountsForScoring)
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
                var currentHandicap = await CalculateAndUpdateCurrentHandicapAsync(playerId);
                suggestions[playerId] = currentHandicap;
            }

            return suggestions;
        }

        /// <summary>
        /// Calculate handicaps for all players in a season based on their scores
        /// </summary>
        /// <param name="seasonId">The season ID</param>
        /// <param name="courseRating">9-hole course rating (default 35)</param>
        /// <param name="slopeRating">Slope rating (default 113)</param>
        /// <returns>Dictionary of player IDs and their calculated handicaps</returns>
        public async Task<Dictionary<Guid, decimal>> CalculateAllPlayerHandicapsAsync(Guid seasonId, int courseRating = 35, decimal slopeRating = 113)
        {
            var results = new Dictionary<Guid, decimal>();
            
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
                        
                        // Optionally update the player's current handicap in the database
                        var player = await _context.Players.FindAsync(playerId);
                        if (player != null)
                        {
                            player.CurrentHandicap = handicap;
                        }
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
