using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HandicapController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly HandicapService _handicapService;

        public HandicapController(AppDbContext context, HandicapService handicapService)
        {
            _context = context;
            _handicapService = handicapService;
        }

        /// <summary>
        /// Get a player's session-specific handicap for a given season and week
        /// </summary>
        [HttpGet("{playerId}/{seasonId}/{weekNumber}")]
        public async Task<ActionResult<decimal>> GetPlayerSessionHandicap(Guid playerId, Guid seasonId, int weekNumber)
        {
            try
            {
                var handicap = await _handicapService.GetPlayerSessionHandicapAsync(playerId, seasonId, weekNumber);
                return Ok(handicap);
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

        /// <summary>
        /// Set session initial handicap for a player
        /// </summary>
        [HttpPost("set")]
        public async Task<ActionResult<PlayerSessionHandicap>> SetPlayerSessionHandicap([FromBody] SetSessionHandicapRequest request)
        {
            try
            {
                var result = await _handicapService.SetPlayerSessionHandicapAsync(
                    request.PlayerId,
                    request.SeasonId,
                    request.SessionStartWeekNumber,
                    request.SessionInitialHandicap);

                return Ok(result);
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

        /// <summary>
        /// Bulk set session handicaps for multiple players
        /// </summary>
        [HttpPost("bulk-set")]
        public async Task<ActionResult<object>> BulkSetSessionHandicaps([FromBody] BulkSetSessionHandicapsRequest request)
        {
            try
            {
                int count = 0;
                foreach (var playerHandicap in request.PlayerHandicaps)
                {
                    await _handicapService.SetPlayerSessionHandicapAsync(
                        playerHandicap.PlayerId,
                        request.SeasonId,
                        request.SessionStartWeekNumber,
                        playerHandicap.SessionInitialHandicap);
                    count++;
                }

                return Ok(new { count });
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

        /// <summary>
        /// Get all session handicaps for a player in a season
        /// </summary>
        [HttpGet("{playerId}/{seasonId}")]
        public async Task<ActionResult<List<PlayerSessionHandicap>>> GetPlayerSessionHandicaps(Guid playerId, Guid seasonId)
        {
            try
            {
                var handicaps = await _context.PlayerSessionHandicaps
                    .Where(psh => psh.PlayerId == playerId && psh.SeasonId == seasonId)
                    .OrderBy(psh => psh.SessionStartWeekNumber)
                    .ToListAsync();

                return Ok(handicaps);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }

    // Request DTOs
    public class SetSessionHandicapRequest
    {
        public Guid PlayerId { get; set; }
        public Guid SeasonId { get; set; }
        public int SessionStartWeekNumber { get; set; }
        public decimal SessionInitialHandicap { get; set; }
    }

    public class BulkSetSessionHandicapsRequest
    {
        public Guid SeasonId { get; set; }
        public int SessionStartWeekNumber { get; set; }
        public List<PlayerHandicapDto> PlayerHandicaps { get; set; } = new();
    }

    public class PlayerHandicapDto
    {
        public Guid PlayerId { get; set; }
        public decimal SessionInitialHandicap { get; set; }
    }
}
