using Microsoft.AspNetCore.Mvc;

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
    }
}
