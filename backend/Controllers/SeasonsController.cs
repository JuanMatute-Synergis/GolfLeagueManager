using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class SeasonsController : ControllerBase
    {
        private readonly SeasonService _seasonService;

        public SeasonsController(SeasonService seasonService)
        {
            _seasonService = seasonService;
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
    }
}
