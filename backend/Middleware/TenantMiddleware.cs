using GolfLeagueManager.Services;

namespace GolfLeagueManager.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<TenantMiddleware> _logger;

        public TenantMiddleware(RequestDelegate next, ILogger<TenantMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }
        public async Task InvokeAsync(HttpContext context, ITenantService tenantService)
        {
            var tenantId = ExtractTenantFromRequest(context);

            // Enhanced logging
            _logger.LogInformation("=== TENANT DETECTION ===");
            _logger.LogInformation("Host: {Host}", context.Request.Host.Host);
            _logger.LogInformation("Headers: {Headers}", string.Join(", ", context.Request.Headers.Select(h => $"{h.Key}: {h.Value}")));
            _logger.LogInformation("Extracted TenantId: {TenantId}", tenantId ?? "NULL");

            if (!string.IsNullOrEmpty(tenantId))
            {
                tenantService.SetCurrentTenant(tenantId);

                // Check if tenant database exists, create if not
                if (!await tenantService.TenantExistsAsync(tenantId))
                {
                    _logger.LogInformation("Creating new tenant database for: {TenantId}", tenantId);
                    await tenantService.CreateTenantDatabaseAsync(tenantId);
                }

                _logger.LogInformation("Request processed for tenant: {TenantId}", tenantId);
            }
            else
            {
                // Use the configured default tenant if no subdomain is detected
                var defaultTenant = tenantService.GetCurrentTenant();
                tenantService.SetCurrentTenant(defaultTenant);
                _logger.LogInformation("No tenant detected, using default: {DefaultTenant}", defaultTenant);
            }

            await _next(context);
        }

        private string? ExtractTenantFromRequest(HttpContext context)
        {
            var host = context.Request.Host.Host;

            // Handle CloudFlare tunnel domains like htlyons.golfleaguemanager.app
            if (host.EndsWith(".golfleaguemanager.app"))
            {
                var subdomain = host.Split('.')[0];
                if (!string.IsNullOrEmpty(subdomain) && subdomain != "golfleaguemanager")
                {
                    return subdomain;
                }
            }

            // Handle localhost development with port
            if (host.StartsWith("localhost") || host.StartsWith("127.0.0.1"))
            {
                // For development, you can check for a custom header or query parameter
                if (context.Request.Headers.ContainsKey("X-Tenant-Id"))
                {
                    return context.Request.Headers["X-Tenant-Id"].ToString();
                }

                // Or check query parameter
                if (context.Request.Query.ContainsKey("tenant"))
                {
                    return context.Request.Query["tenant"].ToString();
                }

                // Return null to use the configured default tenant
                return null;
            }

            return null;
        }
    }
}
