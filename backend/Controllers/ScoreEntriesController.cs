using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScoreEntriesController : ControllerBase
    {
        private readonly ScoreEntryService _scoreEntryService;

        public ScoreEntriesController(ScoreEntryService scoreEntryService)
        {
            _scoreEntryService = scoreEntryService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ScoreEntry>>> GetScoreEntries()
        {
            var scoreEntries = await _scoreEntryService.GetAllScoreEntriesAsync();
            return Ok(scoreEntries);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ScoreEntry>> GetScoreEntry(Guid id)
        {
            var scoreEntry = await _scoreEntryService.GetScoreEntryByIdAsync(id);
            if (scoreEntry == null)
            {
                return NotFound();
            }
            return Ok(scoreEntry);
        }

        [HttpGet("week/{weekId}")]
        public async Task<ActionResult<IEnumerable<ScoreEntry>>> GetScoreEntriesByWeek(Guid weekId)
        {
            var scoreEntries = await _scoreEntryService.GetScoreEntriesByWeekIdAsync(weekId);
            return Ok(scoreEntries);
        }

        [HttpGet("player/{playerId}")]
        public async Task<ActionResult<IEnumerable<ScoreEntry>>> GetScoreEntriesByPlayer(Guid playerId)
        {
            var scoreEntries = await _scoreEntryService.GetScoreEntriesByPlayerIdAsync(playerId);
            return Ok(scoreEntries);
        }

        [HttpGet("week/{weekId}/leaderboard")]
        public async Task<ActionResult<IEnumerable<ScoreEntry>>> GetWeekLeaderboard(Guid weekId)
        {
            var leaderboard = await _scoreEntryService.GetLeaderboardByWeekAsync(weekId);
            return Ok(leaderboard);
        }

        [HttpGet("season/{seasonId}/standings")]
        public async Task<ActionResult<IEnumerable<PlayerSeasonStats>>> GetSeasonStandings(Guid seasonId)
        {
            var standings = await _scoreEntryService.GetSeasonStandingsAsync(seasonId);
            return Ok(standings);
        }

        [HttpPost]
        public async Task<ActionResult<ScoreEntry>> CreateScoreEntry(ScoreEntry scoreEntry)
        {
            try
            {
                var createdScoreEntry = await _scoreEntryService.CreateOrUpdateScoreEntryAsync(scoreEntry);
                return CreatedAtAction(nameof(GetScoreEntry), new { id = createdScoreEntry.Id }, createdScoreEntry);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("bulk")]
        public async Task<ActionResult<IEnumerable<ScoreEntry>>> BulkCreateScoreEntries(IEnumerable<ScoreEntry> scoreEntries)
        {
            var results = await _scoreEntryService.BulkCreateScoreEntriesAsync(scoreEntries);
            return Ok(results);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<ScoreEntry>> UpdateScoreEntry(Guid id, ScoreEntry scoreEntry)
        {
            if (id != scoreEntry.Id)
            {
                return BadRequest("Score entry ID mismatch");
            }

            try
            {
                var updatedScoreEntry = await _scoreEntryService.CreateOrUpdateScoreEntryAsync(scoreEntry);
                return Ok(updatedScoreEntry);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteScoreEntry(Guid id)
        {
            var result = await _scoreEntryService.DeleteScoreEntryAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
