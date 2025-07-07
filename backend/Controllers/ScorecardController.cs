using Microsoft.AspNetCore.Mvc;
using GolfLeagueManager.Business;
using GolfLeagueManager.Models;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScorecardController : ControllerBase
    {
        private readonly ScorecardService _scorecardService;

        public ScorecardController(ScorecardService scorecardService)
        {
            _scorecardService = scorecardService;
        }

        [HttpPost("save")]
        public async Task<ActionResult<ScorecardResponse>> SaveScorecard([FromBody] ScorecardSaveRequest request)
        {
            if (request == null || request.MatchupId == Guid.Empty)
            {
                return BadRequest("Invalid scorecard data");
            }

            var response = await _scorecardService.SaveScorecardAsync(request);
            
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return BadRequest(response);
            }
        }

        [HttpGet("{matchupId}")]
        public async Task<ActionResult<List<HoleScore>>> GetScorecard(Guid matchupId)
        {
            if (matchupId == Guid.Empty)
            {
                return BadRequest("Invalid matchup ID");
            }

            var holeScores = await _scorecardService.GetScorecardAsync(matchupId);
            return Ok(holeScores);
        }

        [HttpGet("{matchupId}/complete")]
        public async Task<ActionResult<ScorecardResponse>> GetCompleteScorecard(Guid matchupId)
        {
            if (matchupId == Guid.Empty)
            {
                return BadRequest("Invalid matchup ID");
            }

            var response = await _scorecardService.GetCompleteScorecardAsync(matchupId);
            
            if (response.Success)
            {
                return Ok(response);
            }
            else
            {
                return NotFound(response);
            }
        }

        [HttpDelete("{matchupId}")]
        public async Task<IActionResult> DeleteScorecard(Guid matchupId)
        {
            if (matchupId == Guid.Empty)
            {
                return BadRequest("Invalid matchup ID");
            }

            var result = await _scorecardService.DeleteScorecardAsync(matchupId);
            
            if (result)
            {
                return NoContent();
            }
            else
            {
                return NotFound("Scorecard not found");
            }
        }

        /// <summary>
        /// Get scorecards for multiple matchups in bulk
        /// </summary>
        /// <param name="matchupIds">List of matchup IDs</param>
        /// <returns>Dictionary of matchup IDs to their hole scores</returns>
        [HttpPost("bulk")]
        public async Task<ActionResult<Dictionary<string, List<HoleScore>>>> GetBulkScorecards([FromBody] List<Guid> matchupIds)
        {
            if (matchupIds == null || !matchupIds.Any())
            {
                return BadRequest("No matchup IDs provided");
            }

            try
            {
                var results = await _scorecardService.GetBulkScorecardsAsync(matchupIds);
                return Ok(results);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error retrieving bulk scorecards: {ex.Message}");
            }
        }
    }
}
