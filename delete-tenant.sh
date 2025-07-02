#!/bin/bash

# Tenant Deletion Script for Golf League Manager
# Usage: ./delete-tenant.sh <tenant_name>

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

print_usage() {
    echo "Usage: $0 <tenant_name>"
    echo ""
    echo "Examples:"
    echo "  $0 johndoe        # Deletes database golfdb_johndoe"
    echo "  $0 smithleague    # Deletes database golfdb_smithleague"
    echo ""
    echo -e "${RED}WARNING: This will permanently delete all data for the tenant!${NC}"
}

check_database_exists() {
    local db_name="$1"
    
    docker exec golfleague_postgres psql -U "$PGUSER" -d "$MASTER_DB" \
        -t -c "SELECT 1 FROM pg_database WHERE datname='$db_name';" | grep -q 1
}

delete_tenant_database() {
    local tenant_name="$1"
    local db_name="golfdb_$tenant_name"
    
    echo -e "${BLUE}Deleting tenant database: $db_name${NC}"
    
    # Terminate any active connections to the database
    docker exec golfleague_postgres psql -U "$PGUSER" -d "$MASTER_DB" \
        -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$db_name';"
    
    # Drop the database
    docker exec golfleague_postgres psql -U "$PGUSER" -d "$MASTER_DB" \
        -c "DROP DATABASE \"$db_name\";"
    
    echo -e "${GREEN}✓ Database deleted successfully${NC}"
}

remove_dns_route() {
    local tenant_name="$1"
    
    echo -e "${BLUE}Removing DNS route for: $tenant_name.golfleaguemanager.app${NC}"
    
    # Remove DNS route using cloudflared
    cloudflared tunnel route dns golf-league-manager "$tenant_name.golfleaguemanager.app" --overwrite-dns=false || {
        echo -e "${YELLOW}⚠ Could not remove DNS route automatically${NC}"
        echo -e "${YELLOW}You may need to remove it manually from Cloudflare dashboard${NC}"
    }
    
    echo -e "${GREEN}✓ DNS route removal attempted${NC}"
}

main() {
    if [[ $# -ne 1 ]]; then
        print_usage
        exit 1
    fi
    
    local tenant_name="$1"
    local db_name="golfdb_$tenant_name"
    
    echo -e "${YELLOW}=== Golf League Manager - Tenant Deletion ===${NC}"
    echo -e "${RED}WARNING: This will permanently delete all data for tenant: $tenant_name${NC}"
    echo -e "${RED}Database: $db_name${NC}"
    echo -e "${RED}Subdomain: $tenant_name.golfleaguemanager.app${NC}"
    echo ""
    
    # Check if database exists
    if ! check_database_exists "$db_name"; then
        echo -e "${YELLOW}Database '$db_name' does not exist${NC}"
        exit 0
    fi
    
    # Confirmation prompt
    echo -e "${YELLOW}Are you sure you want to delete tenant '$tenant_name'? (type 'DELETE' to confirm):${NC}"
    read -r confirmation
    
    if [[ "$confirmation" != "DELETE" ]]; then
        echo -e "${BLUE}Deletion cancelled${NC}"
        exit 0
    fi
    
    # Delete the tenant database
    delete_tenant_database "$tenant_name"
    
    # Remove DNS route
    remove_dns_route "$tenant_name"
    
    echo ""
    echo -e "${GREEN}=== Tenant Deletion Complete! ===${NC}"
    echo -e "${GREEN}Tenant '$tenant_name' has been completely removed${NC}"
    echo ""
}

# Check if required tools are installed
command -v docker >/dev/null 2>&1 || { echo -e "${RED}Error: docker is required but not installed${NC}" >&2; exit 1; }
command -v cloudflared >/dev/null 2>&1 || { echo -e "${RED}Error: cloudflared is required but not installed${NC}" >&2; exit 1; }

# Run main function
main "$@"
