# Creating a New Tenant - Step by Step Guide

## Overview

The Golf League Manager supports multi-tenancy, allowing you to create completely isolated instances for different golf leagues. Each tenant gets its own database, admin user, and subdomain access.

## Prerequisites

Before creating a new tenant, ensure:

- ✅ Docker is running
- ✅ PostgreSQL container (`golfleague_postgres`) is running
- ✅ Backend application is running
- ✅ CloudFlare tunnel is configured (for external access)

### Quick Prerequisites Check

```bash
# Check if Docker is running
docker ps | grep golfleague_postgres

# Check if backend is running
curl -s http://localhost:5505/health

# Check CloudFlare tunnel (optional)
cloudflared tunnel list
```

## Method 1: Using the Automated Script (Recommended)

### Step 1: Run the Tenant Creation Script

```bash
cd /Users/juanmatute/Projects/GolfLeagueManager
./create-tenant.sh <tenant_name>
```

**Example:**
```bash
./create-tenant.sh mybusiness
```

### Step 2: Script Execution Flow

The script will automatically:

1. **Validate the tenant name**
   - Must be 2-50 characters
   - Only alphanumeric characters and hyphens
   - Will be used as subdomain

2. **Create the tenant database**
   - Database name: `golfdb_mybusiness`
   - Uses existing PostgreSQL container

3. **Apply database migrations**
   - Creates all necessary tables
   - Applies the latest schema

4. **Create admin user**
   - Username: `admin`
   - Password: `golfpassword`
   - Admin privileges: Yes

5. **Setup DNS routing**
   - Adds CloudFlare DNS route
   - Enables `mybusiness.golfleaguemanager.app`

### Step 3: Verify Creation

```bash
# List all tenants
./list-tenants.sh

# Test the new tenant
curl -H "X-Tenant-Id: mybusiness" http://localhost:5505/api/tenant/current
```

## Method 2: Manual Creation (Advanced)

If you need more control or want to understand the process:

### Step 1: Create the Database

```bash
# Connect to PostgreSQL container
docker exec golfleague_postgres psql -U golfuser -d golfdb -c "CREATE DATABASE \"golfdb_mybusiness\";"

# Verify database creation
docker exec golfleague_postgres psql -U golfuser -l | grep mybusiness
```

### Step 2: Apply Migrations

```bash
cd backend
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=golfdb_mybusiness;Username=golfuser;Password=golfpassword"
dotnet ef database update
cd ..
```

### Step 3: Create Admin User

```bash
python3 create-admin-user.py mybusiness
```

### Step 4: Setup DNS Route (Optional)

```bash
cloudflared tunnel route dns golf-league-manager mybusiness.golfleaguemanager.app
```

## Method 3: Testing New Tenant

### Local Testing

1. **Test tenant detection:**
   ```bash
   curl -s -H "X-Tenant-Id: mybusiness" http://localhost:5505/api/tenant/current
   ```

2. **Test authentication:**
   ```bash
   curl -s -X POST \
       -H "Content-Type: application/json" \
       -H "X-Tenant-Id: mybusiness" \
       -d '{"username":"admin","password":"golfpassword"}' \
       http://localhost:5505/api/auth/login
   ```

### External Testing (if DNS is configured)

Visit: `https://mybusiness.golfleaguemanager.app`

## Access Your New Tenant

### Development Access

**Option 1: Using Headers**
```bash
# Add this header to any API request:
X-Tenant-Id: mybusiness
```

**Option 2: Using /etc/hosts (for frontend testing)**
```bash
# Add to /etc/hosts:
127.0.0.1 mybusiness.golfleaguemanager.app

# Then visit:
http://mybusiness.golfleaguemanager.app:4500
```

### Production Access

Once DNS is configured:
- **URL**: `https://mybusiness.golfleaguemanager.app`
- **Username**: `admin`
- **Password**: `golfpassword`

## Common Tenant Names and Use Cases

### Business/Organization Names
```bash
./create-tenant.sh acmegolf
./create-tenant.sh westside
./create-tenant.sh riverside
```

### Location-Based
```bash
./create-tenant.sh dallas
./create-tenant.sh chicago
./create-tenant.sh miami
```

### Personal/Family Leagues
```bash
./create-tenant.sh smithfamily
./create-tenant.sh johnsleague
./create-tenant.sh weekend-warriors
```

## Managing Your Tenants

### List All Tenants
```bash
./list-tenants.sh
```

### Test All Tenants
```bash
./test-multi-tenant-comprehensive.sh
```

### Delete a Tenant (if needed)
```bash
./delete-tenant.sh mybusiness
```

## Troubleshooting

### Common Issues

**Issue: "Database already exists"**
```bash
# Solution: Delete the existing tenant first
./delete-tenant.sh mybusiness
./create-tenant.sh mybusiness
```

**Issue: "PostgreSQL container not running"**
```bash
# Solution: Start the services
./manage-services.sh start
```

**Issue: "CloudFlared not found"**
```bash
# Solution: Install CloudFlare tunnel or skip DNS setup
# The tenant will still work locally
```

**Issue: "Migrations failed"**
```bash
# Solution: Check if backend builds successfully
cd backend
dotnet build
cd ..
```

### Debug Commands

```bash
# Check database exists
docker exec golfleague_postgres psql -U golfuser -l | grep mybusiness

# Check admin user exists
docker exec golfleague_postgres psql -U golfuser -d golfdb_mybusiness -c "SELECT * FROM \"Users\";"

# Check tenant detection
curl -H "X-Tenant-Id: mybusiness" http://localhost:5505/api/tenant/current
```

## Best Practices

### Naming Conventions

✅ **Good tenant names:**
- `acmegolf`
- `west-side`
- `dallas-league`
- `weekend-warriors`

❌ **Bad tenant names:**
- `Acme Golf` (spaces not allowed)
- `acme_golf` (underscores not recommended)
- `a` (too short)

### Security Considerations

1. **Change the default admin password** after initial setup
2. **Use HTTPS in production** (set `Secure: true` in cookies)
3. **Consider firewall rules** for database access
4. **Regular backups** of tenant databases

### Development Workflow

1. **Create a test tenant** for development:
   ```bash
   ./create-tenant.sh devtest
   ```

2. **Use headers for local testing**:
   ```bash
   X-Tenant-Id: devtest
   ```

3. **Clean up test tenants** when done:
   ```bash
   ./delete-tenant.sh devtest
   ```

## Example: Complete New Tenant Setup

Let's create a tenant for "Riverside Golf League":

```bash
# Step 1: Create the tenant
./create-tenant.sh riverside

# Step 2: Verify creation
./list-tenants.sh | grep riverside

# Step 3: Test locally
curl -H "X-Tenant-Id: riverside" http://localhost:5505/api/tenant/current

# Step 4: Test authentication
curl -X POST \
    -H "Content-Type: application/json" \
    -H "X-Tenant-Id: riverside" \
    -d '{"username":"admin","password":"golfpassword"}' \
    http://localhost:5505/api/auth/login

# Step 5: Access externally (if DNS configured)
# Visit: https://riverside.golfleaguemanager.app
```

## Summary

Creating a new tenant is simple with the automated script:

1. **Run**: `./create-tenant.sh <name>`
2. **Access**: Use headers for local testing or subdomain for production
3. **Login**: `admin` / `golfpassword`
4. **Manage**: Use the provided management scripts

Each tenant is completely isolated with its own database, users, and data. The same application code serves all tenants seamlessly.

---

**Need help?** 
- Run `./test-multi-tenant-comprehensive.sh` to verify your setup
- Check the logs in `/tmp/backend.log` for debugging
- Use `./list-tenants.sh` to see all available tenants
