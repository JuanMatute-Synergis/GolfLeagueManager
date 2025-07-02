using Microsoft.EntityFrameworkCore;
using GolfLeagueManager.Services;

namespace GolfLeagueManager.Data
{
    public interface ITenantDbContextFactory
    {
        AppDbContext CreateDbContext();
    }

    public class TenantDbContextFactory : ITenantDbContextFactory
    {
        private readonly ITenantService _tenantService;
        private readonly IConfiguration _configuration;

        public TenantDbContextFactory(ITenantService tenantService, IConfiguration configuration)
        {
            _tenantService = tenantService;
            _configuration = configuration;
        }

        public AppDbContext CreateDbContext()
        {
            var tenantId = _tenantService.GetCurrentTenant();
            var connectionString = _tenantService.GetConnectionString(tenantId);

            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseNpgsql(connectionString);

            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
