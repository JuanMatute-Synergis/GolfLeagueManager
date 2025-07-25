using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GolfLeagueManager.Services;

namespace GolfLeagueManager.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TenantController : ControllerBase
    {
        private readonly ITenantService _tenantService;
        private readonly ILogger<TenantController> _logger;

        public TenantController(ITenantService tenantService, ILogger<TenantController> logger)
        {
            _tenantService = tenantService;
            _logger = logger;
        }

        [HttpGet("current")]
        public IActionResult GetCurrentTenant()
        {
            var tenantId = _tenantService.GetCurrentTenant();
            return Ok(new { tenantId = tenantId });
        }

        [HttpPost("create/{tenantId}")]
        [Authorize] // Require authentication to create new tenants
        public async Task<IActionResult> CreateTenant(string tenantId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    return BadRequest("Tenant ID cannot be empty");
                }

                // Validate tenant ID format (alphanumeric and hyphens only)
                if (!System.Text.RegularExpressions.Regex.IsMatch(tenantId, @"^[a-zA-Z0-9-]+$"))
                {
                    return BadRequest("Tenant ID can only contain letters, numbers, and hyphens");
                }

                if (await _tenantService.TenantExistsAsync(tenantId))
                {
                    return Conflict($"Tenant '{tenantId}' already exists");
                }

                await _tenantService.CreateTenantDatabaseAsync(tenantId);

                _logger.LogInformation("Created new tenant: {TenantId}", tenantId);

                return Ok(new { message = $"Tenant '{tenantId}' created successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant: {TenantId}", tenantId);
                return StatusCode(500, $"Error creating tenant: {ex.Message}");
            }
        }

        [HttpGet("exists/{tenantId}")]
        public async Task<IActionResult> TenantExists(string tenantId)
        {
            try
            {
                var exists = await _tenantService.TenantExistsAsync(tenantId);
                return Ok(new { tenantId = tenantId, exists = exists });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking tenant existence: {TenantId}", tenantId);
                return StatusCode(500, $"Error checking tenant: {ex.Message}");
            }
        }

        /// <summary>
        /// Switch to a different tenant for the current session (Development only)
        /// </summary>
        [HttpPost("switch/{tenantId}")]
        public async Task<IActionResult> SwitchTenant(string tenantId)
        {
            try
            {
                // Only allow in development environment for safety
                var environment = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                if (!environment.IsDevelopment())
                {
                    return BadRequest(new { error = "Tenant switching only allowed in development environment" });
                }

                var exists = await _tenantService.TenantExistsAsync(tenantId);
                if (!exists)
                {
                    return BadRequest(new { error = $"Tenant '{tenantId}' does not exist" });
                }

                _tenantService.SetCurrentTenant(tenantId);
                _logger.LogInformation("Switched to tenant: {TenantId}", tenantId);

                return Ok(new { tenantId, message = "Switched to tenant successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error switching to tenant {TenantId}", tenantId);
                return StatusCode(500, new { error = "Error switching tenant" });
            }
        }
    }
}
