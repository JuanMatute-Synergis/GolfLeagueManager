using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/matchups")]
    public class MatchupsController : ControllerBase
    {
        private readonly MatchupService _matchupService;

        public MatchupsController(MatchupService matchupService)
        {
            _matchupService = matchupService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Matchup>>> GetAllMatchups()
        {
            var matchups = await _matchupService.GetAllMatchupsAsync();
            return Ok(matchups);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Matchup>> GetMatchup(Guid id)
        {
            var matchup = await _matchupService.GetMatchupByIdAsync(id);
            if (matchup == null)
                return NotFound();

            return Ok(matchup);
        }

        [HttpGet("week/{weekId}")]
        public async Task<ActionResult<IEnumerable<Matchup>>> GetMatchupsByWeek(Guid weekId)
        {
            var matchups = await _matchupService.GetMatchupsByWeekIdAsync(weekId);
            return Ok(matchups);
        }

        [HttpGet("season/{seasonId}")]
        public async Task<ActionResult<IEnumerable<Matchup>>> GetMatchupsBySeason(Guid seasonId)
        {
            var matchups = await _matchupService.GetMatchupsBySeasonIdAsync(seasonId);
            return Ok(matchups);
        }

        [HttpGet("player/{playerId}")]
        public async Task<ActionResult<IEnumerable<Matchup>>> GetMatchupsByPlayer(Guid playerId)
        {
            var matchups = await _matchupService.GetMatchupsByPlayerIdAsync(playerId);
            return Ok(matchups);
        }

        [HttpPost]
        public async Task<ActionResult<Matchup>> CreateMatchup(Matchup matchup)
        {
            try
            {
                var createdMatchup = await _matchupService.CreateMatchupAsync(matchup);
                return Created($"/api/matchups/{createdMatchup.Id}", createdMatchup);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Matchup>> UpdateMatchup(Guid id, Matchup matchup)
        {
            if (id != matchup.Id)
                return BadRequest("Matchup ID mismatch");

            try
            {
                var updatedMatchup = await _matchupService.UpdateMatchupAsync(matchup);
                return Ok(updatedMatchup);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMatchup(Guid id)
        {
            var deleted = await _matchupService.DeleteMatchupAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }

        [HttpPost("generate/{weekId}")]
        public async Task<ActionResult<IEnumerable<Matchup>>> GenerateMatchupsForWeek(Guid weekId, [FromQuery] Guid seasonId)
        {
            try
            {
                var matchups = await _matchupService.GenerateRandomMatchupsForWeekAsync(weekId, seasonId);
                return Ok(matchups);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("generate-round-robin")]
        public async Task<ActionResult<IEnumerable<Matchup>>> GenerateRoundRobinMatchups([FromBody] GenerateMatchupsRequest request)
        {
            try
            {
                var matchups = await _matchupService.GenerateRandomMatchupsForWeekAsync(request.WeekId, request.SeasonId);
                return Ok(matchups);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    public class GenerateMatchupsRequest
    {
        public Guid WeekId { get; set; }
        public Guid SeasonId { get; set; }
    }
}
