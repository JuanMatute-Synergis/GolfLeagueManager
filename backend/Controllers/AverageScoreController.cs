using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AverageScoreController : ControllerBase
    {
        private readonly AverageScoreService _averageScoreService;
        private readonly AppDbContext _context;

        public AverageScoreController(AverageScoreService averageScoreService, AppDbContext context)
        {
            _averageScoreService = averageScoreService;
            _context = context;
        }

        /// <summary>
        /// Get scoring statistics for a player in a specific season
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="seasonId">Season ID</param>
        /// <returns>Player scoring statistics</returns>
        [HttpGet("player/{playerId}/season/{seasonId}/stats")]
        public async Task<ActionResult<PlayerScoringStats>> GetPlayerStats(Guid playerId, Guid seasonId)
        {
            try
            {
                var stats = await _averageScoreService.GetPlayerScoringStatsAsync(playerId, seasonId);
                return Ok(stats);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving player stats: {ex.Message}");
            }
        }

        /// <summary>
        /// Update average score for a specific player in a season
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="seasonId">Season ID</param>
        /// <returns>Updated average score</returns>
        [HttpPost("player/{playerId}/season/{seasonId}/update")]
        public async Task<ActionResult<decimal>> UpdatePlayerAverageScore(Guid playerId, Guid seasonId)
        {
            try
            {
                var updatedAverage = await _averageScoreService.UpdatePlayerAverageScoreAsync(playerId, seasonId);
                return Ok(new { playerId, seasonId, currentAverageScore = updatedAverage });
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating player average score: {ex.Message}");
            }
        }

        /// <summary>
        /// Update average scores for all players who played in a specific week
        /// </summary>
        /// <param name="weekId">Week ID</param>
        /// <returns>Dictionary of updated average scores by player ID</returns>
        [HttpPost("week/{weekId}/update-all")]
        public async Task<ActionResult<Dictionary<Guid, decimal>>> UpdateAverageScoresForWeek(Guid weekId)
        {
            try
            {
                var results = await _averageScoreService.UpdateAverageScoresForWeekAsync(weekId);
                return Ok(results);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error updating average scores for week: {ex.Message}");
            }
        }

        /// <summary>
        /// Recalculate all player average scores for a season
        /// </summary>
        /// <param name="seasonId">Season ID</param>
        /// <returns>Dictionary of recalculated average scores by player ID</returns>
        [HttpPost("season/{seasonId}/recalculate-all")]
        public async Task<ActionResult<Dictionary<Guid, decimal>>> RecalculateAllAverageScoresForSeason(Guid seasonId)
        {
            try
            {
                var results = await _averageScoreService.RecalculateAllAverageScoresForSeasonAsync(seasonId);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error recalculating average scores for season: {ex.Message}");
            }
        }

        /// <summary>
        /// Get scoring statistics for all players in a season
        /// </summary>
        /// <param name="seasonId">Season ID</param>
        /// <returns>List of player scoring statistics</returns>
        [HttpGet("season/{seasonId}/all-stats")]
        public ActionResult<List<PlayerScoringStats>> GetAllPlayerStatsForSeason(Guid seasonId)
        {
            try
            {
                // This would require getting all players in the season first
                // For now, return a placeholder - this could be expanded later
                return Ok(new List<PlayerScoringStats>());
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving all player stats for season: {ex.Message}");
            }
        }

        /// <summary>
        /// Get a player's average score up to (but not including) a specific week.
        /// This method automatically determines which session the week belongs to and uses 
        /// the appropriate session's initial average as the baseline for calculations.
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="seasonId">Season ID</param>
        /// <param name="weekNumber">Week number (exclusive)</param>
        /// <returns>Average score up to the specified week using session-aware calculation</returns>
        [HttpGet("player/{playerId}/season/{seasonId}/uptoweek/{weekNumber}")]
        public async Task<ActionResult<decimal>> GetPlayerAverageScoreUpToWeek(Guid playerId, Guid seasonId, int weekNumber)
        {
            try
            {
                var average = await _averageScoreService.GetPlayerAverageScoreUpToWeekAsync(playerId, seasonId, weekNumber);
                return Ok(average);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving player average score up to week: {ex.Message}");
            }
        }

        // ========== SESSION MANAGEMENT ENDPOINTS ==========
        // Session averages are automatically handled by GetPlayerAverageScoreUpToWeek
        // These endpoints are for managing session initial averages

        /// <summary>
        /// Set or update a session initial average for a player.
        /// This is used by administrative tools to set baseline averages for each session.
        /// The GetPlayerAverageScoreUpToWeek method will automatically use the appropriate session average.
        /// </summary>
        /// <param name="playerId">Player ID</param>
        /// <param name="seasonId">Season ID</param>
        /// <param name="sessionStartWeekNumber">Session start week number</param>
        /// <param name="sessionInitialAverage">The initial average for this session</param>
        /// <returns>Success message</returns>
        [HttpPost("player/{playerId}/season/{seasonId}/session/{sessionStartWeekNumber}/initial")]
        public async Task<ActionResult> SetPlayerSessionInitialAverage(
            Guid playerId, 
            Guid seasonId, 
            int sessionStartWeekNumber, 
            [FromBody] decimal sessionInitialAverage)
        {
            // Verify player exists
            var player = await _context.Players.FindAsync(playerId);
            if (player == null)
            {
                return BadRequest($"Player with ID {playerId} not found");
            }

            // Verify season exists
            var season = await _context.Seasons.FindAsync(seasonId);
            if (season == null)
            {
                return BadRequest($"Season with ID {seasonId} not found");
            }

            // Verify the session start week exists and is marked as session start
            var sessionStartWeek = await _context.Weeks
                .Where(w => w.SeasonId == seasonId && 
                           w.WeekNumber == sessionStartWeekNumber && 
                           w.SessionStart)
                .FirstOrDefaultAsync();
            
            if (sessionStartWeek == null)
            {
                return BadRequest($"Week {sessionStartWeekNumber} is not a valid session start week for season {seasonId}");
            }

            // Find existing session average or create new one
            var existingSessionAverage = await _context.PlayerSessionAverages
                .Where(psa => psa.PlayerId == playerId && 
                             psa.SeasonId == seasonId && 
                             psa.SessionStartWeekNumber == sessionStartWeekNumber)
                .FirstOrDefaultAsync();

            if (existingSessionAverage != null)
            {
                // Update existing
                existingSessionAverage.SessionInitialAverage = sessionInitialAverage;
                existingSessionAverage.ModifiedDate = DateTime.UtcNow;
            }
            else
            {
                // Create new
                var newSessionAverage = new PlayerSessionAverage
                {
                    PlayerId = playerId,
                    SeasonId = seasonId,
                    SessionStartWeekNumber = sessionStartWeekNumber,
                    SessionInitialAverage = sessionInitialAverage,
                    CreatedDate = DateTime.UtcNow
                };

                _context.PlayerSessionAverages.Add(newSessionAverage);
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Session initial average set successfully", sessionInitialAverage });
        }

        /// <summary>
        /// Set session initial averages for all players in a season for a specific session start week.
        /// This is a convenience endpoint to bulk-set session averages.
        /// </summary>
        /// <param name="seasonId">Season ID</param>
        /// <param name="sessionStartWeekNumber">Session start week number</param>
        /// <param name="defaultSessionAverage">The default initial average to use for all players</param>
        /// <returns>Number of players updated</returns>
        [HttpPost("season/{seasonId}/session/{sessionStartWeekNumber}/bulk")]
        public async Task<ActionResult<object>> SetSessionAveragesForAllPlayers(
            Guid seasonId, 
            int sessionStartWeekNumber, 
            [FromBody] decimal defaultSessionAverage)
        {
            try
            {
                var updatedCount = await _averageScoreService.SetSessionAveragesForAllPlayersAsync(
                    seasonId, sessionStartWeekNumber, defaultSessionAverage);

                return Ok(new { 
                    message = $"Session averages set for {updatedCount} players",
                    playersUpdated = updatedCount,
                    seasonId,
                    sessionStartWeekNumber,
                    defaultSessionAverage
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
