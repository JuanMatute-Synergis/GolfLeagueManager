using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        private readonly PlayerService _playerService;

        public PlayersController(PlayerService playerService)
        {
            _playerService = playerService;
        }

        [HttpPost]
        public IActionResult AddPlayer(Player player)
        {
            _playerService.AddPlayer(player);
            return Created($"/players/{player.Id}", player);
        }

        [HttpGet]
        public ActionResult<IEnumerable<Player>> GetPlayers()
        {
            var players = _playerService.GetPlayers();
            return Ok(players);
        }

        [HttpPut("{id}")]
        public IActionResult UpdatePlayer(Guid id, Player player)
        {
            if (id != player.Id)
                return BadRequest("Player ID mismatch");

            var updated = _playerService.UpdatePlayer(player);
            if (!updated)
                return NotFound();
            
            return NoContent();
        }

        [HttpDelete("{id}")]
        public IActionResult DeletePlayer(Guid id)
        {
            var deleted = _playerService.DeletePlayer(id);
            if (!deleted)
                return NotFound();
            return NoContent();
        }

        [HttpGet("season/{seasonId}/flights")]
        public ActionResult<IEnumerable<PlayerWithFlight>> GetPlayersInFlights(Guid seasonId)
        {
            var players = _playerService.GetPlayersInFlights(seasonId);
            return Ok(players);
        }
    }
}
