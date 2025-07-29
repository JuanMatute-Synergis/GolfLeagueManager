using Microsoft.AspNetCore.Mvc;
using GolfLeagueManager.Business;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/flight-assignment-migration")]
    public class FlightAssignmentMigrationController : ControllerBase
    {
        private readonly FlightAssignmentMigrationService _migrationService;

        public FlightAssignmentMigrationController(FlightAssignmentMigrationService migrationService)
        {
            _migrationService = migrationService;
        }

        [HttpPost("migrate-existing")]
        public async Task<IActionResult> MigrateExistingAssignments()
        {
            try
            {
                var migratedCount = await _migrationService.MigrateExistingFlightAssignmentsAsync();
                return Ok(new { message = $"Successfully migrated {migratedCount} flight assignments", migratedCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("copy-to-session")]
        public async Task<IActionResult> CopyAssignmentsToNewSession([FromBody] CopyToSessionRequest request)
        {
            try
            {
                var copiedCount = await _migrationService.CopyFlightAssignmentsToNewSessionAsync(
                    request.SeasonId,
                    request.NewSessionStartWeekNumber);

                return Ok(new { message = $"Successfully copied {copiedCount} flight assignments to new session", copiedCount });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("session-start-week")]
        public async Task<IActionResult> GetSessionStartWeekForWeek([FromQuery] Guid seasonId, [FromQuery] int weekNumber)
        {
            try
            {
                var sessionStartWeek = await _migrationService.GetSessionStartWeekNumberForWeekAsync(seasonId, weekNumber);
                return Ok(new { sessionStartWeekNumber = sessionStartWeek });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }

    public class CopyToSessionRequest
    {
        public Guid SeasonId { get; set; }
        public int NewSessionStartWeekNumber { get; set; }
    }
}
