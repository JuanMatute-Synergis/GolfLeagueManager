using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class HealthController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HealthController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetHealth()
        {
            try
            {
                // Check database connectivity
                await _context.Database.CanConnectAsync();

                var healthStatus = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    database = "connected",
                    version = "1.0.0"
                };

                return Ok(healthStatus);
            }
            catch (Exception ex)
            {
                var healthStatus = new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    database = "disconnected",
                    error = ex.Message,
                    version = "1.0.0"
                };

                return StatusCode(503, healthStatus);
            }
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedHealth()
        {
            try
            {
                var canConnect = await _context.Database.CanConnectAsync();
                var playerCount = await _context.Players.CountAsync();
                var seasonCount = await _context.Seasons.CountAsync();

                var detailedHealth = new
                {
                    status = "healthy",
                    timestamp = DateTime.UtcNow,
                    database = new
                    {
                        connected = canConnect,
                        playerCount,
                        seasonCount
                    },
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    version = "1.0.0",
                    uptime = Environment.TickCount64
                };

                return Ok(detailedHealth);
            }
            catch (Exception ex)
            {
                var detailedHealth = new
                {
                    status = "unhealthy",
                    timestamp = DateTime.UtcNow,
                    error = ex.Message,
                    environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Unknown",
                    version = "1.0.0"
                };

                return StatusCode(503, detailedHealth);
            }
        }
    }
}
