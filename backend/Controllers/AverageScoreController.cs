using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AverageScoreController : ControllerBase
    {
        private readonly AverageScoreService _averageScoreService;

        public AverageScoreController(AverageScoreService averageScoreService)
        {
            _averageScoreService = averageScoreService;
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
    }
}
