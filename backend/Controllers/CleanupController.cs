using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CleanupController : ControllerBase
    {
        private readonly DatabaseCleanupService _cleanupService;

        public CleanupController(DatabaseCleanupService cleanupService)
        {
            _cleanupService = cleanupService;
        }

        [HttpDelete("all-scores")]
        public async Task<ActionResult<CleanupResult>> DeleteAllScores()
        {
            var result = await _cleanupService.DeleteAllScoresAsync();
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }

        [HttpDelete("week/{weekNumber}/scores")]
        public async Task<ActionResult<CleanupResult>> DeleteWeekScores(int weekNumber)
        {
            var result = await _cleanupService.DeleteScoresForWeekAsync(weekNumber);
            
            if (result.Success)
            {
                return Ok(result);
            }
            else
            {
                return BadRequest(result);
            }
        }
    }
}
