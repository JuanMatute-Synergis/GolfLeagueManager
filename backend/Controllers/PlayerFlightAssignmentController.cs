using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/player-flight-assignments")]
    public class PlayerFlightAssignmentController : ControllerBase
    {
        private readonly PlayerFlightAssignmentService _service;

        public PlayerFlightAssignmentController(PlayerFlightAssignmentService service)
        {
            _service = service;
        }

        [HttpPost]
        public ActionResult<PlayerFlightAssignment> AddAssignment(PlayerFlightAssignment assignment)
        {
            try
            {
                _service.AddAssignment(assignment);
                return Created($"/api/player-flight-assignments/{assignment.Id}", assignment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("flight/{flightId}")]
        public ActionResult<IEnumerable<PlayerFlightAssignment>> GetAssignmentsByFlight(Guid flightId)
        {
            var assignments = _service.GetAssignmentsByFlight(flightId);
            return Ok(assignments);
        }

        [HttpGet("flight/{flightId}/session")]
        public ActionResult<IEnumerable<PlayerFlightAssignment>> GetAssignmentsByFlightAndSession(Guid flightId, [FromQuery] Guid seasonId, [FromQuery] int sessionStartWeekNumber)
        {
            if (seasonId == Guid.Empty || sessionStartWeekNumber <= 0)
                return BadRequest("Valid seasonId and sessionStartWeekNumber are required.");

            var assignments = _service.GetAssignmentsByFlightAndSession(flightId, seasonId, sessionStartWeekNumber);
            return Ok(assignments);
        }

        [HttpGet("player/{playerId}")]
        public ActionResult<IEnumerable<PlayerFlightAssignment>> GetAssignmentsByPlayer(Guid playerId)
        {
            var assignments = _service.GetAssignmentsByPlayer(playerId);
            return Ok(assignments);
        }

        [HttpGet("player/{playerId}/season/{seasonId}")]
        public ActionResult<IEnumerable<PlayerFlightAssignment>> GetAssignmentsByPlayerAndSeason(Guid playerId, Guid seasonId)
        {
            var assignments = _service.GetAssignmentsByPlayerAndSeason(playerId, seasonId);
            return Ok(assignments);
        }

        [HttpGet("session")]
        public ActionResult<IEnumerable<PlayerFlightAssignment>> GetAssignmentsBySession([FromQuery] Guid seasonId, [FromQuery] int sessionStartWeekNumber)
        {
            if (seasonId == Guid.Empty || sessionStartWeekNumber <= 0)
                return BadRequest("Valid seasonId and sessionStartWeekNumber are required.");

            var assignments = _service.GetAssignmentsBySession(seasonId, sessionStartWeekNumber);
            return Ok(assignments);
        }

        [HttpGet("player/{playerId}/session")]
        public ActionResult<PlayerFlightAssignment> GetPlayerAssignmentForSession(Guid playerId, [FromQuery] Guid seasonId, [FromQuery] int sessionStartWeekNumber)
        {
            if (seasonId == Guid.Empty || sessionStartWeekNumber <= 0)
                return BadRequest("Valid seasonId and sessionStartWeekNumber are required.");

            var assignment = _service.GetPlayerAssignmentForSession(playerId, seasonId, sessionStartWeekNumber);
            if (assignment == null)
                return NotFound();

            return Ok(assignment);
        }

        [HttpGet("{id}")]
        public ActionResult<PlayerFlightAssignment> GetAssignmentById(Guid id)
        {
            var assignment = _service.GetAssignmentById(id);
            if (assignment == null)
                return NotFound();

            return Ok(assignment);
        }

        [HttpPut("{id}")]
        public ActionResult<PlayerFlightAssignment> UpdateAssignment(Guid id, PlayerFlightAssignment assignment)
        {
            if (id != assignment.Id)
                return BadRequest("Assignment ID mismatch");

            try
            {
                var updated = _service.UpdateAssignment(assignment);
                if (!updated)
                    return NotFound();

                return Ok(assignment);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public IActionResult RemoveAssignment(Guid id)
        {
            var removed = _service.RemoveAssignment(id);
            if (!removed)
                return NotFound();

            return NoContent();
        }

        [HttpGet]
        public ActionResult<IEnumerable<PlayerFlightAssignment>> GetAllAssignments()
        {
            var assignments = _service.GetAllAssignments();
            return Ok(assignments);
        }
    }
}
