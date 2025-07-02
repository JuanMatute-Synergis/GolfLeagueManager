using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeagueSettingsController : ControllerBase
    {
        private readonly LeagueSettingsService _leagueSettingsService;

        public LeagueSettingsController(LeagueSettingsService leagueSettingsService)
        {
            _leagueSettingsService = leagueSettingsService;
        }

        [HttpGet("season/{seasonId}")]
        public async Task<ActionResult<LeagueSettings>> GetLeagueSettings(Guid seasonId)
        {
            try
            {
                var settings = await _leagueSettingsService.GetLeagueSettingsAsync(seasonId);
                return Ok(settings);
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

        [HttpPut("season/{seasonId}")]
        public async Task<ActionResult<LeagueSettings>> UpdateLeagueSettings(Guid seasonId, [FromBody] LeagueSettings settings)
        {
            try
            {
                if (settings.SeasonId != seasonId)
                {
                    return BadRequest("Season ID in URL does not match settings.");
                }

                var updatedSettings = await _leagueSettingsService.UpdateLeagueSettingsAsync(settings);
                return Ok(updatedSettings);
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

        [HttpPost("season/{seasonId}/reset")]
        public async Task<ActionResult<LeagueSettings>> ResetToDefaults(Guid seasonId)
        {
            try
            {
                var defaultSettings = await _leagueSettingsService.ResetToDefaultsAsync(seasonId);
                return Ok(defaultSettings);
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

        [HttpGet("enums/handicap-methods")]
        public ActionResult<object> GetHandicapMethods()
        {
            var methods = Enum.GetValues<HandicapCalculationMethod>()
                .Select(e => new { value = (int)e, name = e.ToString() })
                .ToArray();
            return Ok(methods);
        }

        [HttpGet("enums/scoring-methods")]
        public ActionResult<object> GetScoringMethods()
        {
            var methods = Enum.GetValues<ScoringMethod>()
                .Select(e => new { value = (int)e, name = e.ToString() })
                .ToArray();
            return Ok(methods);
        }

        [HttpGet("enums/points-systems")]
        public ActionResult<object> GetPointsSystems()
        {
            var systems = Enum.GetValues<PointsSystem>()
                .Select(e => new { value = (int)e, name = e.ToString() })
                .ToArray();
            return Ok(systems);
        }
    }
}
