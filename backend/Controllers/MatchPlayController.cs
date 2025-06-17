using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/match-play")]
    public class MatchPlayController : ControllerBase
    {
        private readonly MatchPlayService _matchPlayService;

        public MatchPlayController(MatchPlayService matchPlayService)
        {
            _matchPlayService = matchPlayService;
        }

        [HttpPost("{matchupId}/calculate")]
        public async Task<ActionResult> CalculateMatchPlayResults(Guid matchupId)
        {
            try
            {
                var success = await _matchPlayService.CalculateMatchPlayResultsAsync(matchupId);
                
                if (!success)
                {
                    return NotFound("Matchup not found or no hole scores available");
                }

                return Ok(new { message = "Match play results calculated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating match play results: {ex.Message}");
            }
        }

        [HttpPost("{matchupId}/initialize")]
        public async Task<ActionResult> InitializeHoleScores(Guid matchupId)
        {
            try
            {
                var success = await _matchPlayService.InitializeHoleScoresAsync(matchupId);
                
                if (!success)
                {
                    return BadRequest("Failed to initialize hole scores");
                }

                return Ok(new { message = "Hole scores initialized successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest($"Error initializing hole scores: {ex.Message}");
            }
        }
    }
}
