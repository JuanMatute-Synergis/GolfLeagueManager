using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonsController : ControllerBase
    {
        private readonly SeasonService _seasonService;
        private readonly WeekService _weekService;

        public SeasonsController(SeasonService seasonService, WeekService weekService)
        {
            _seasonService = seasonService;
            _weekService = weekService;
        }

        [HttpPost]
        public IActionResult AddSeason(Season season)
        {
            try
            {
                _seasonService.AddSeason(season);
                return Created($"/api/seasons/{season.Id}", season);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<IEnumerable<Season>> GetSeasons()
        {
            var seasons = _seasonService.GetSeasons();
            return Ok(seasons);
        }

        [HttpGet("{id}")]
        public ActionResult<Season> GetSeasonById(Guid id)
        {
            var season = _seasonService.GetSeasonById(id);
            if (season == null)
                return NotFound();
            
            return Ok(season);
        }

        [HttpGet("active")]
        public ActionResult<IEnumerable<Season>> GetActiveSeasons()
        {
            var seasons = _seasonService.GetActiveSeasons();
            return Ok(seasons);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateSeason(Guid id, Season season)
        {
            if (id != season.Id)
                return BadRequest("Season ID mismatch");

            try
            {
                var updated = _seasonService.UpdateSeason(season);
                if (!updated)
                    return NotFound();
                
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteSeason(Guid id)
        {
            var deleted = _seasonService.DeleteSeason(id);
            if (!deleted)
                return NotFound();
            
            return NoContent();
        }

        [HttpPost("{id}/regenerate-weeks")]
        public async Task<IActionResult> RegenerateWeeks(Guid id)
        {
            try
            {
                // First, delete all existing weeks for this season
                var existingWeeks = await _weekService.GetWeeksBySeasonIdAsync(id);
                foreach (var week in existingWeeks)
                {
                    await _weekService.DeleteWeekAsync(week.Id);
                }

                // Then regenerate weeks using the updated logic
                await _weekService.GenerateWeeksForSeasonAsync(id);
                
                return Ok(new { message = "Weeks regenerated successfully" });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error regenerating weeks", error = ex.Message });
            }
        }
    }
}
