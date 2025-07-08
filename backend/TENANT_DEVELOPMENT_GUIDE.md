# Tenant Development Guide

This document explains how to work with different tenants during development.

## Setting Default Tenant

### Method 1: Command Line Arguments
```bash
# Run with a specific tenant
dotnet run --DefaultTenant="mytenant"

# Or use the helper scripts
./run-with-tenant.sh mytenant           # Linux/Mac
.\run-with-tenant.ps1 mytenant          # Windows
```

### Method 2: Environment Variables
```bash
# Set environment variable
export DefaultTenant="mytenant"
dotnet run

# Or on Windows
set DefaultTenant=mytenant
dotnet run
```

### Method 3: Configuration Files
Update `appsettings.Development.json`:
```json
{
  "DefaultTenant": "mytenant"
}
```

## Tenant Debugging Endpoints

The following endpoints are available for development (only work in Development environment):

### Get Current Tenant
```
GET /api/tenant/current
```
Returns the currently active tenant ID.

### Check if Tenant Exists
```
GET /api/tenant/exists/{tenantId}
```
Checks if a tenant database exists.

### Create New Tenant
```
POST /api/tenant/create/{tenantId}
```
Creates a new tenant database with all migrations applied.

### Switch Tenant (Development Only)
```
POST /api/tenant/switch/{tenantId}
```
Switches the current session to a different tenant (only works in Development environment).

## URL-based Tenant Selection

### For Local Development
Add query parameter or header:
- Query: `http://localhost:5274/api/players?tenant=mytenant`
- Header: `X-Tenant-Id: mytenant`

### For Production
Use subdomains:
- `https://mytenant.golfleaguemanager.app`

## Database Naming Convention

Tenant databases follow the pattern: `golfdb_{tenantId}`

Examples:
- `golfdb_htlyons`
- `golfdb_southmoore`
- `golfdb_mytenant`

## Priority Order

The system determines the tenant in this order:
1. URL subdomain (production)
2. X-Tenant-Id header (development)
3. ?tenant= query parameter (development)
4. Configured DefaultTenant
5. "htlyons" (fallback)

## Example Usage

```bash
# Start with southmoore tenant
./run-with-tenant.sh southmoore

# Check current tenant
curl http://localhost:5274/api/tenant/current

# Create a new tenant
curl -X POST http://localhost:5274/api/tenant/create/newtenant

# Switch to the new tenant
curl -X POST http://localhost:5274/api/tenant/switch/newtenant

# Verify the switch
curl http://localhost:5274/api/tenant/current
```
