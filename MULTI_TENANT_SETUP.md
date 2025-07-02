# Multi-Tenant Golf League Manager

## Overview
The Golf League Manager now supports multi-tenancy based on subdomains. Each subdomain gets its own separate database while sharing the same application code.

## How it Works

### Tenant Detection
- **Subdomain-based**: `tenant.golfleaguemanager.app` routes to the `tenant` database
- **Development Support**: Use `X-Tenant-Id` header or `?tenant=` query parameter for local testing
- **Default Tenant**: If no tenant is detected, defaults to `htlyons`

### Database Isolation
- Each tenant gets a separate PostgreSQL database: `golfdb_{tenant_id}`
- Automatic database creation when a new tenant is accessed
- Automatic migration execution for new tenant databases

### Supported Subdomains
- `htlyons.golfleaguemanager.app` → `golfdb_htlyons`
- `testclient.golfleaguemanager.app` → `golfdb_testclient`
- Any subdomain will automatically create its database on first access

## API Endpoints

### Tenant Management
- `GET /api/tenant/current` - Get current tenant ID
- `POST /api/tenant/create/{tenantId}` - Create new tenant (requires authentication)
- `GET /api/tenant/exists/{tenantId}` - Check if tenant exists

## Testing Multi-Tenancy

### 1. Access Different Tenants
```bash
# Access htlyons tenant
curl https://htlyons.golfleaguemanager.app

# Access testclient tenant
curl https://testclient.golfleaguemanager.app
```

### 2. Check Tenant Detection (Local Development)
```bash
# Test with header
curl -H "X-Tenant-Id: htlyons" http://localhost:5505/api/tenant/current

# Test with query parameter
curl "http://localhost:5505/api/tenant/current?tenant=testclient"
```

### 3. Database Verification
Each tenant will have its own database. You can verify this by connecting to PostgreSQL:
```bash
# Connect to PostgreSQL
docker exec -it golfleague_postgres psql -U golfuser -d postgres

# List all databases
\l

# You should see:
# - golfdb (original/default)
# - golfdb_htlyons
# - golfdb_testclient
# - etc.
```

## CloudFlared Configuration
The CloudFlared tunnel is configured to support wildcard subdomains:
```yaml
ingress:
  - hostname: "*.golfleaguemanager.app"
    service: http://localhost:4500
  - hostname: golfleaguemanager.app
    service: http://localhost:4500
  - service: http_status:404
```

## Development Notes

### Adding New Tenants
1. **Automatic**: Just access `{tenant}.golfleaguemanager.app` - database will be created automatically
2. **Manual**: Use the API endpoint `POST /api/tenant/create/{tenantId}` (requires authentication)

### DNS Configuration
Add new subdomains to CloudFlared:
```bash
cloudflared tunnel route dns golf-league-manager {subdomain}.golfleaguemanager.app
```

### Local Development
For local testing, use headers or query parameters:
- Header: `X-Tenant-Id: {tenant}`
- Query: `?tenant={tenant}`

## Architecture Components

### Backend Changes
- `TenantMiddleware`: Detects tenant from subdomain/headers
- `TenantService`: Manages tenant databases and connections
- `TenantDbContextFactory`: Creates tenant-specific DbContext instances
- Multi-tenant aware dependency injection

### Database Strategy
- **Database-per-tenant**: Each tenant gets a separate database
- **Shared application**: Single application instance serves all tenants
- **Automatic provisioning**: New databases created on-demand

## Security Considerations
- Tenant isolation at the database level
- Tenant context validated on every request
- Automatic creation limited to valid subdomain patterns
- Authentication required for administrative tenant operations
