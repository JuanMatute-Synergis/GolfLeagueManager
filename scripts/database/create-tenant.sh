#!/bin/bash

# Tenant Setup Script for Golf League Manager
# Usage: ./create-tenant.sh <tenant_name>

set -e

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Database connection settings
PGUSER="golfuser"
PGPASSWORD="golfpassword"
PGHOST="localhost"
PGPORT="5432"
MASTER_DB="golfdb"

# Admin user settings
ADMIN_USERNAME="admin"
ADMIN_PASSWORD="golfpassword"

print_usage() {
    echo "Usage: $0 <tenant_name>"
    echo ""
    echo "Examples:"
    echo "  $0 johndoe        # Creates database golfdb_johndoe"
    echo "  $0 smithleague    # Creates database golfdb_smithleague"
    echo ""
    echo "The tenant name should be:"
    echo "  - Alphanumeric characters and hyphens only"
    echo "  - Lowercase recommended"
    echo "  - Will be used as subdomain: <tenant>.golfleaguemanager.app"
}

validate_tenant_name() {
    local tenant_name="$1"
    
    if [[ ! "$tenant_name" =~ ^[a-zA-Z0-9-]+$ ]]; then
        echo -e "${RED}Error: Tenant name can only contain letters, numbers, and hyphens${NC}"
        return 1
    fi
    
    if [[ ${#tenant_name} -lt 2 ]]; then
        echo -e "${RED}Error: Tenant name must be at least 2 characters long${NC}"
        return 1
    fi
    
    if [[ ${#tenant_name} -gt 50 ]]; then
        echo -e "${RED}Error: Tenant name must be less than 50 characters${NC}"
        return 1
    fi
    
    return 0
}

check_database_exists() {
    local db_name="$1"
    
    docker exec golfleague_postgres psql -U "$PGUSER" -d "$MASTER_DB" \
        -t -c "SELECT 1 FROM pg_database WHERE datname='$db_name';" | grep -q 1
}

create_tenant_database() {
    local tenant_name="$1"
    local db_name="golfdb_$tenant_name"
    
    echo -e "${BLUE}Creating tenant database: $db_name${NC}"
    
    # Create the database using Docker
    docker exec golfleague_postgres psql -U "$PGUSER" -d "$MASTER_DB" \
        -c "CREATE DATABASE \"$db_name\";"
    
    echo -e "${GREEN}✓ Database created successfully${NC}"
}

run_migrations() {
    local tenant_name="$1"
    local db_name="golfdb_$tenant_name"
    
    echo -e "${BLUE}Running migrations for tenant: $tenant_name${NC}"
    
    # Navigate to backend directory and run migrations
    cd "$(dirname "$0")/../../backend"
    
    # Set connection string for the tenant database (using Docker host)
    export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=$db_name;Username=$PGUSER;Password=$PGPASSWORD"
    
    # Run EF Core migrations
    dotnet ef database update
    
    # Go back to original directory
    cd "$(dirname "$0")"
    
    echo -e "${GREEN}✓ Migrations completed successfully${NC}"
}

create_admin_user() {
    local tenant_name="$1"
    
    echo -e "${BLUE}Creating admin user for tenant: $tenant_name${NC}"
    
    # Use the external Python script to create admin user
    if python3 "$(dirname "$0")/create-admin-user.py" "$tenant_name"; then
        echo -e "${GREEN}✓ Admin user created successfully${NC}"
    else
        echo -e "${RED}✗ Failed to create admin user${NC}"
        return 1
    fi
}


setup_dns_route() {
    local tenant_name="$1"
    
    echo -e "${BLUE}Setting up DNS route for: $tenant_name.golfleaguemanager.app${NC}"
    
    # Add DNS route using cloudflared container
    if docker ps | grep -q cloudflared; then
        docker exec cloudflared cloudflared tunnel route dns golf-league-manager "$tenant_name.golfleaguemanager.app"
        echo -e "${GREEN}✓ DNS route configured${NC}"
    else
        echo -e "${YELLOW}⚠ Cloudflared container not running. DNS route not configured.${NC}"
        echo -e "${YELLOW}Please configure DNS manually or start cloudflared container.${NC}"
    fi
}

setup_default_course() {
    local tenant_name="$1"
    
    echo -e "${BLUE}Setting up default course for tenant: $tenant_name${NC}"
    
    # Check if Southmoore course import script exists and import it
    if [[ -f "$(dirname "$0")/import_southmoore_course.py" ]]; then
        echo -e "${BLUE}Importing Southmoore Golf Course...${NC}"
        if python3 "$(dirname "$0")/import_southmoore_course.py" "$tenant_name"; then
            echo -e "${GREEN}✓ Southmoore Golf Course imported successfully${NC}"
            
            # Set it as default course
            echo -e "${BLUE}Setting Southmoore as default course...${NC}"
            if python3 "$(dirname "$0")/set_default_course.py" "$tenant_name"; then
                echo -e "${GREEN}✓ Southmoore Golf Course set as default${NC}"
            else
                echo -e "${YELLOW}⚠ Could not set Southmoore as default course${NC}"
            fi
        else
            echo -e "${YELLOW}⚠ Could not import Southmoore Golf Course${NC}"
        fi
    else
        echo -e "${YELLOW}⚠ Southmoore course import script not found${NC}"
    fi
}

main() {
    if [[ $# -ne 1 ]]; then
        print_usage
        exit 1
    fi
    
    local tenant_name="$1"
    local db_name="golfdb_$tenant_name"
    
    echo -e "${YELLOW}=== Golf League Manager - Tenant Setup ===${NC}"
    echo -e "${YELLOW}Tenant Name: $tenant_name${NC}"
    echo -e "${YELLOW}Database: $db_name${NC}"
    echo -e "${YELLOW}Subdomain: $tenant_name.golfleaguemanager.app${NC}"
    echo ""
    
    # Validate tenant name
    if ! validate_tenant_name "$tenant_name"; then
        exit 1
    fi
    
    # Check if database already exists
    if check_database_exists "$db_name"; then
        echo -e "${RED}Error: Database '$db_name' already exists${NC}"
        echo -e "${YELLOW}If you want to recreate it, please delete it first:${NC}"
        echo -e "${YELLOW}  ./delete-tenant.sh $tenant_name${NC}"
        exit 1
    fi
    
    # Create the tenant database
    create_tenant_database "$tenant_name"
    
    # Run migrations
    run_migrations "$tenant_name"
    
    # Create admin user
    create_admin_user "$tenant_name"
    
    # Setup default course
    setup_default_course "$tenant_name"
    
    # Setup DNS route
    setup_dns_route "$tenant_name"
    
    echo ""
    echo -e "${GREEN}=== Tenant Setup Complete! ===${NC}"
    echo -e "${GREEN}Tenant: $tenant_name${NC}"
    echo -e "${GREEN}URL: https://$tenant_name.golfleaguemanager.app${NC}"
    echo -e "${GREEN}Admin Username: $ADMIN_USERNAME${NC}"
    echo -e "${GREEN}Admin Password: $ADMIN_PASSWORD${NC}"
    echo ""
    echo -e "${YELLOW}You can now access the application at:${NC}"
    echo -e "${BLUE}https://$tenant_name.golfleaguemanager.app${NC}"
    echo ""
    echo -e "${YELLOW}Login with:${NC}"
    echo -e "${BLUE}Username: $ADMIN_USERNAME${NC}"
    echo -e "${BLUE}Password: $ADMIN_PASSWORD${NC}"
}

# Check if required tools are installed
command -v docker >/dev/null 2>&1 || { echo -e "${RED}Error: docker is required but not installed${NC}" >&2; exit 1; }
command -v dotnet >/dev/null 2>&1 || { echo -e "${RED}Error: dotnet is required but not installed${NC}" >&2; exit 1; }
command -v python3 >/dev/null 2>&1 || { echo -e "${RED}Error: python3 is required but not installed${NC}" >&2; exit 1; }

# Check if PostgreSQL container is running
if ! docker ps | grep -q golfleague_postgres; then
    echo -e "${RED}Error: PostgreSQL container 'golfleague_postgres' is not running${NC}" >&2
    echo -e "${YELLOW}Please start the services with: ./manage-services.sh start${NC}" >&2
    exit 1
fi

# Run main function
main "$@"
