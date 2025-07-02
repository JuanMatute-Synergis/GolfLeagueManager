# Multi-Tenant Database Management Guide

## Overview
This guide covers how to handle database schema changes in the multi-tenant Golf League Manager system.

## How New Tables Are Handled

### ✅ **For NEW Tenants** (Automatic)
When a new tenant accesses the system for the first time:
1. **TenantMiddleware** detects the new tenant
2. **TenantService.CreateTenantDatabaseAsync()** creates the database
3. **RunMigrationsForTenantAsync()** applies ALL migrations automatically
4. ✅ **Result**: New tenant gets ALL tables including latest schema changes

### 🔧 **For EXISTING Tenants** (Manual Process Required)

When you add new tables/migrations, existing tenant databases need to be updated manually.

## Migration Strategies

### **Option 1: Automated Script (Recommended)**

Use the provided `migrate-all-tenants.sh` script:

```bash
# Make sure you're in the project root
cd /Users/juanmatute/Projects/GolfLeagueManager

# Run the migration script
./migrate-all-tenants.sh
```

**What it does:**
- Finds all tenant databases (`golfdb_*`)
- Applies Entity Framework migrations to each
- Reports success/failure for each tenant

### **Option 2: Manual SQL Script**

For specific schema changes (like LeagueSettings table):

```bash
# Apply to specific tenant
docker exec -i golfleague_postgres psql -U golfuser -d golfdb_TENANT_NAME < backend/create_league_settings_table.sql

# Example for testclient tenant
docker exec -i golfleague_postgres psql -U golfuser -d golfdb_testclient < backend/create_league_settings_table.sql
```

### **Option 3: Individual Entity Framework Migrations**

For each tenant manually:

```bash
cd backend

# Set connection string for specific tenant
export ConnectionStrings__DefaultConnection="Host=localhost;Database=golfdb_TENANT_NAME;Username=golfuser;Password=golfpassword"

# Run migrations
dotnet ef database update
```

## Verification Commands

### Check Existing Tenant Databases
```bash
docker exec golfleague_postgres psql -U golfuser -d postgres -c "SELECT datname FROM pg_database WHERE datname LIKE 'golfdb_%';"
```

### Verify Table Exists in Tenant Database
```bash
docker exec golfleague_postgres psql -U golfuser -d golfdb_TENANT_NAME -c "\dt" | grep TABLE_NAME
```

### Check Migration History
```bash
docker exec golfleague_postgres psql -U golfuser -d golfdb_TENANT_NAME -c "SELECT \"MigrationId\" FROM \"__EFMigrationsHistory\" ORDER BY \"MigrationId\";"
```

## Best Practices

### **When Adding New Tables:**

1. **Create Migration**: 
   ```bash
   cd backend
   dotnet ef migrations add YourMigrationName
   ```

2. **Test on Development Database First**:
   ```bash
   dotnet ef database update
   ```

3. **Apply to All Existing Tenants**:
   ```bash
   ./migrate-all-tenants.sh
   ```

4. **Verify Application**:
   - Test API endpoints with different tenants
   - Use `X-Tenant-Id` header for local testing

### **Current Tenant Databases:**
As of July 1, 2025:
- `golfdb_htlyons` (main/default tenant)
- `golfdb_testclient` (test tenant)
- `golfdb_default` (fallback tenant)

### **Example: Testing Multi-Tenant API**

```bash
# Test with htlyons tenant (default)
curl "http://localhost:5274/api/LeagueSettings/enums/handicap-methods"

# Test with testclient tenant
curl -H "X-Tenant-Id: testclient" "http://localhost:5274/api/LeagueSettings/enums/handicap-methods"

# Test with custom tenant
curl -H "X-Tenant-Id: customtenant" "http://localhost:5274/api/LeagueSettings/enums/handicap-methods"
```

## Troubleshooting

### **Problem**: New table doesn't exist in tenant database
**Solution**: Run the migration script or apply SQL manually

### **Problem**: Migration script fails for a tenant
**Solution**: Check the tenant database exists and connection string is correct

### **Problem**: Entity Framework says "up to date" but table missing
**Solution**: Use manual SQL script approach

## Automation Opportunities

### **Future Enhancement**: Runtime Migration Check
Consider adding automatic migration checking to TenantMiddleware:

```csharp
// In TenantMiddleware.InvokeAsync
if (!await tenantService.TenantExistsAsync(tenantId))
{
    await tenantService.CreateTenantDatabaseAsync(tenantId);
}
else
{
    // Check if migrations are needed
    await tenantService.EnsureMigrationsAppliedAsync(tenantId);
}
```

This would automatically apply missing migrations when tenants access the system.

## Summary

- **New tenants**: ✅ Automatic (handled by TenantService)
- **Existing tenants**: 🔧 Manual process required
- **Best approach**: Use `migrate-all-tenants.sh` script
- **Verification**: Always test API with different tenant headers
- **Future**: Consider automatic migration detection

The current system provides a good balance of automation for new tenants while requiring explicit action for existing tenant updates (which is safer for production systems).
