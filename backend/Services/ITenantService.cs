namespace GolfLeagueManager.Services
{
    public interface ITenantService
    {
        string GetCurrentTenant();
        void SetCurrentTenant(string tenantId);
        string GetConnectionString(string tenantId);
        Task<bool> TenantExistsAsync(string tenantId);
        Task CreateTenantDatabaseAsync(string tenantId);
    }
}
