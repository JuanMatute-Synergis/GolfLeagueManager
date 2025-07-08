using Microsoft.AspNetCore.Mvc;
using System.Linq;

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
        public async Task<ActionResult<LeagueSettings>> UpdateLeagueSettings(Guid seasonId, [FromBody] UpdateLeagueSettingsRequest request)
        {
            try
            {
                // Log the received data for debugging
                System.Console.WriteLine($"Received seasonId: {seasonId}");
                System.Console.WriteLine($"Received request: {request?.GetType().Name ?? "null"}");

                if (request == null)
                {
                    return BadRequest("Settings object is required.");
                }

                if (!ModelState.IsValid)
                {
                    var errors = ModelState
                        .Where(x => x.Value != null && x.Value.Errors.Count > 0)
                        .Select(x => new { Field = x.Key, Errors = x.Value?.Errors.Select(e => e.ErrorMessage) ?? new List<string>() })
                        .ToArray();
                    return BadRequest(new { Message = "Validation failed", ValidationErrors = errors });
                }

                // Convert DTO to LeagueSettings
                var settings = new LeagueSettings
                {
                    SeasonId = seasonId,
                    LeagueName = request.LeagueName,
                    HandicapMethod = request.HandicapMethod,
                    AverageMethod = request.AverageMethod,
                    LegacyInitialWeight = request.LegacyInitialWeight,
                    CoursePar = request.CoursePar,
                    CourseRating = request.CourseRating,
                    SlopeRating = request.SlopeRating,
                    MaxRoundsForHandicap = request.MaxRoundsForHandicap,
                    ScoringMethod = request.ScoringMethod,
                    PointsSystem = request.PointsSystem,
                    HoleWinPoints = request.HoleWinPoints,
                    HoleHalvePoints = request.HoleHalvePoints,
                    MatchWinBonus = request.MatchWinBonus,
                    MatchTiePoints = request.MatchTiePoints,
                    UseSessionHandicaps = request.UseSessionHandicaps,
                    AllowHandicapUpdates = request.AllowHandicapUpdates,
                    CustomRules = request.CustomRules
                };

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

        [HttpGet("enums/average-methods")]
        public ActionResult<object> GetAverageMethods()
        {
            var methods = Enum.GetValues<AverageCalculationMethod>()
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
