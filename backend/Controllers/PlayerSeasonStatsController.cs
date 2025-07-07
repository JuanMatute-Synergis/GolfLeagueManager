using Microsoft.AspNetCore.Mvc;
using GolfLeagueManager.Business;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/player-season-stats")]
    public class PlayerSeasonStatsController : ControllerBase
    {
        private readonly PlayerSeasonStatsService _playerSeasonStatsService;

        public PlayerSeasonStatsController(PlayerSeasonStatsService playerSeasonStatsService)
        {
            _playerSeasonStatsService = playerSeasonStatsService;
        }

        [HttpPost]
        public async Task<ActionResult<PlayerSeasonRecord>> CreatePlayerSeasonRecord([FromBody] PlayerSeasonRecord record)
        {
            try
            {
                var createdRecord = await _playerSeasonStatsService.UpdateInitialValuesAsync(
                    record.PlayerId,
                    record.SeasonId,
                    record.InitialHandicap,
                    record.InitialAverageScore);
                return CreatedAtAction(nameof(GetPlayerSeasonRecord),
                    new { playerId = record.PlayerId, seasonId = record.SeasonId },
                    createdRecord);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPut("player/{playerId}/season/{seasonId}")]
        public async Task<ActionResult<PlayerSeasonRecord>> UpdatePlayerSeasonRecord(Guid playerId, Guid seasonId, [FromBody] PlayerSeasonRecord record)
        {
            if (playerId != record.PlayerId || seasonId != record.SeasonId)
            {
                return BadRequest("Player ID or Season ID mismatch");
            }

            try
            {
                var updatedRecord = await _playerSeasonStatsService.UpdateInitialValuesAsync(
                    record.PlayerId,
                    record.SeasonId,
                    record.InitialHandicap,
                    record.InitialAverageScore);
                return Ok(updatedRecord);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("player/{playerId}/season/{seasonId}")]
        public async Task<ActionResult<PlayerSeasonRecord>> GetPlayerSeasonRecord(Guid playerId, Guid seasonId)
        {
            try
            {
                var record = await _playerSeasonStatsService.GetPlayerSeasonStatsAsync(playerId, seasonId);
                if (record == null)
                {
                    return NotFound();
                }
                return Ok(record);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpGet("season/{seasonId}")]
        public async Task<ActionResult<List<PlayerSeasonRecord>>> GetSeasonRecordsBySeason(Guid seasonId)
        {
            try
            {
                var records = await _playerSeasonStatsService.GetSeasonStatsAsync(seasonId);
                return Ok(records);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpDelete("player/{playerId}/season/{seasonId}")]
        public async Task<ActionResult> DeletePlayerSeasonRecord(Guid playerId, Guid seasonId)
        {
            try
            {
                await _playerSeasonStatsService.DeletePlayerSeasonStatsAsync(playerId, seasonId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
