using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WeeksController : ControllerBase
    {
        private readonly WeekService _weekService;

        public WeeksController(WeekService weekService)
        {
            _weekService = weekService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Week>>> GetWeeks()
        {
            var weeks = await _weekService.GetAllWeeksAsync();
            return Ok(weeks);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Week>> GetWeek(Guid id)
        {
            var week = await _weekService.GetWeekByIdAsync(id);
            if (week == null)
            {
                return NotFound();
            }
            return Ok(week);
        }

        [HttpGet("season/{seasonId}")]
        public async Task<ActionResult<IEnumerable<Week>>> GetWeeksBySeason(Guid seasonId)
        {
            var weeks = await _weekService.GetWeeksBySeasonIdAsync(seasonId);
            return Ok(weeks);
        }

        [HttpGet("season/{seasonId}/current")]
        public async Task<ActionResult<Week>> GetCurrentWeek(Guid seasonId)
        {
            var week = await _weekService.GetCurrentWeekAsync(seasonId);
            if (week == null)
            {
                return NotFound();
            }
            return Ok(week);
        }

        [HttpGet("season/{seasonId}/next")]
        public async Task<ActionResult<Week>> GetNextWeek(Guid seasonId)
        {
            var week = await _weekService.GetNextWeekAsync(seasonId);
            if (week == null)
            {
                return NotFound();
            }
            return Ok(week);
        }

        [HttpPost]
        public async Task<ActionResult<Week>> CreateWeek(Week week)
        {
            try
            {
                var createdWeek = await _weekService.CreateWeekAsync(week);
                return CreatedAtAction(nameof(GetWeek), new { id = createdWeek.Id }, createdWeek);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Week>> UpdateWeek(Guid id, Week week)
        {
            if (id != week.Id)
            {
                return BadRequest("Week ID mismatch");
            }

            try
            {
                var updatedWeek = await _weekService.UpdateWeekAsync(week);
                return Ok(updatedWeek);
            }
            catch (ArgumentException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWeek(Guid id)
        {
            var result = await _weekService.DeleteWeekAsync(id);
            if (!result)
            {
                return NotFound();
            }
            return NoContent();
        }
    }
}
