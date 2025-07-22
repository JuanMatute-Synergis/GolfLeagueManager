using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace GolfLeagueManager.Services
{
    public class TenantService : ITenantService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<TenantService> _logger;
        private readonly IServiceProvider _serviceProvider;
        private string? _currentTenant;
        private readonly string _defaultTenant;

        public TenantService(IConfiguration configuration, ILogger<TenantService> logger, IServiceProvider serviceProvider)
        {
            _configuration = configuration;
            _logger = logger;
            _serviceProvider = serviceProvider;

            // Get default tenant from configuration (command line, environment, or appsettings)
            _defaultTenant = _configuration["DefaultTenant"] ?? "htlyons";
            _logger.LogInformation("TenantService initialized with default tenant: {DefaultTenant}", _defaultTenant);
        }

        public string GetCurrentTenant()
        {
            return _currentTenant ?? _defaultTenant;
        }

        public void SetCurrentTenant(string tenantId)
        {
            _currentTenant = tenantId;
        }

        public string GetConnectionString(string tenantId)
        {
            var baseConnectionString = _configuration.GetConnectionString("DefaultConnection")!;
            var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);

            // Use tenant-specific database name
            builder.Database = $"golfdb_{tenantId}";

            return builder.ToString();
        }

        public async Task<bool> TenantExistsAsync(string tenantId)
        {
            try
            {
                var connectionString = GetConnectionString(tenantId);
                await using var connection = new NpgsqlConnection(connectionString);
                await connection.OpenAsync();
                return true;
            }
            catch (PostgresException ex) when (ex.SqlState == "3D000") // Database does not exist
            {
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if tenant {TenantId} exists", tenantId);
                return false;
            }
        }

        public async Task CreateTenantDatabaseAsync(string tenantId)
        {
            try
            {
                var baseConnectionString = _configuration.GetConnectionString("DefaultConnection")!;
                var builder = new NpgsqlConnectionStringBuilder(baseConnectionString);
                var masterDbName = builder.Database;
                var tenantDbName = $"golfdb_{tenantId}";

                // Connect to master database to create tenant database
                builder.Database = masterDbName;
                await using var masterConnection = new NpgsqlConnection(builder.ToString());
                await masterConnection.OpenAsync();

                // Create the tenant database
                var createDbCommand = $"CREATE DATABASE \"{tenantDbName}\"";
                await using var command = new NpgsqlCommand(createDbCommand, masterConnection);
                await command.ExecuteNonQueryAsync();

                _logger.LogInformation("Created tenant database: {TenantDbName}", tenantDbName);

                // Run migrations on the new tenant database
                await RunMigrationsForTenantAsync(tenantId);
            }
            catch (PostgresException ex) when (ex.SqlState == "42P04") // Database already exists
            {
                _logger.LogInformation("Tenant database already exists for: {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating tenant database for: {TenantId}", tenantId);
                throw;
            }
        }

        private async Task RunMigrationsForTenantAsync(string tenantId)
        {
            try
            {
                var connectionString = GetConnectionString(tenantId);
                var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
                optionsBuilder.UseNpgsql(connectionString);

                await using var context = new AppDbContext(optionsBuilder.Options);
                await context.Database.MigrateAsync();

                _logger.LogInformation("Migrations completed for tenant: {TenantId}", tenantId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running migrations for tenant: {TenantId}", tenantId);
                throw;
            }
        }
    }
}
