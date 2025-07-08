using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using GolfLeagueManager.Services;
using GolfLeagueManager.Models;
using System.Security.Claims;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LeagueRulesController : ControllerBase
    {
        private readonly ILeagueRulesService _leagueRulesService;

        public LeagueRulesController(ILeagueRulesService leagueRulesService)
        {
            _leagueRulesService = leagueRulesService;
        }

        /// <summary>
        /// Get league rules for a specific season
        /// </summary>
        [HttpGet("{seasonId}")]
        public async Task<ActionResult<LeagueRules>> GetRulesBySeasonId(Guid seasonId)
        {
            try
            {
                var rules = await _leagueRulesService.GetRulesBySeasonIdAsync(seasonId);
                
                if (rules == null)
                {
                    // Return empty rules with default content if none exist
                    return Ok(new LeagueRules
                    {
                        Id = Guid.Empty,
                        SeasonId = seasonId,
                        Content = "<h1>League Rules</h1><p>No rules have been set for this season yet.</p>",
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    });
                }

                return Ok(rules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving the rules.", details = ex.Message });
            }
        }

        /// <summary>
        /// Create or update league rules for a specific season
        /// </summary>
        [HttpPut("{seasonId}")]
        public async Task<ActionResult<LeagueRules>> UpdateRules(Guid seasonId, [FromBody] UpdateRulesRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var username = User.FindFirst(ClaimTypes.Name)?.Value;
                var rules = await _leagueRulesService.CreateOrUpdateRulesAsync(seasonId, request.Content, username);

                return Ok(rules);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while updating the rules.", details = ex.Message });
            }
        }

        /// <summary>
        /// Delete league rules for a specific season
        /// </summary>
        [HttpDelete("{seasonId}")]
        public async Task<ActionResult> DeleteRules(Guid seasonId)
        {
            try
            {
                await _leagueRulesService.DeleteRulesAsync(seasonId);
                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the rules.", details = ex.Message });
            }
        }
    }
}
