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
    }
}
