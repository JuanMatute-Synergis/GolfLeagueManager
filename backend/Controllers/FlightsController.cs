using Microsoft.AspNetCore.Mvc;

namespace GolfLeagueManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class FlightsController : ControllerBase
    {
        private readonly FlightService _flightService;

        public FlightsController(FlightService flightService)
        {
            _flightService = flightService;
        }

        [HttpPost]
        public IActionResult AddFlight(Flight flight)
        {
            try
            {
                _flightService.AddFlight(flight);
                return Created($"/api/flights/{flight.Id}", flight);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet]
        public ActionResult<IEnumerable<Flight>> GetFlights()
        {
            var flights = _flightService.GetFlights();
            return Ok(flights);
        }

        [HttpGet("{id}")]
        public ActionResult<Flight> GetFlightById(Guid id)
        {
            var flight = _flightService.GetFlightById(id);
            if (flight == null)
                return NotFound();
            
            return Ok(flight);
        }

        [HttpGet("active")]
        public ActionResult<IEnumerable<Flight>> GetActiveFlights()
        {
            var flights = _flightService.GetActiveFlights();
            return Ok(flights);
        }

        [HttpGet("upcoming")]
        public ActionResult<IEnumerable<Flight>> GetUpcomingFlights()
        {
            var flights = _flightService.GetUpcomingFlights();
            return Ok(flights);
        }

        [HttpGet("date-range")]
        public ActionResult<IEnumerable<Flight>> GetFlightsByDateRange(
            [FromQuery] DateTime startDate, 
            [FromQuery] DateTime endDate)
        {
            var flights = _flightService.GetFlightsByDateRange(startDate, endDate);
            return Ok(flights);
        }

        [HttpPut("{id}")]
        public IActionResult UpdateFlight(Guid id, Flight flight)
        {
            if (id != flight.Id)
                return BadRequest("Flight ID mismatch");

            try
            {
                var updated = _flightService.UpdateFlight(flight);
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
        public IActionResult DeleteFlight(Guid id)
        {
            var deleted = _flightService.DeleteFlight(id);
            if (!deleted)
                return NotFound();
            
            return NoContent();
        }
    }
}
