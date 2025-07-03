#!/bin/bash

# List Tenants Script for Golf League Manager
# Usage: ./list-tenants.sh

# Colors for output
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

echo -e "${YELLOW}=== Golf League Manager - Tenant List ===${NC}"
echo ""

# Get list of databases that match the tenant pattern
tenant_dbs=$(docker exec golfleague_postgres psql -U "$PGUSER" -d "$MASTER_DB" \
    -t -c "SELECT datname FROM pg_database WHERE datname LIKE 'golfdb_%' ORDER BY datname;" | sed 's/^ *//')

if [[ -z "$tenant_dbs" ]]; then
    echo -e "${BLUE}No tenant databases found${NC}"
    echo ""
    echo -e "${YELLOW}Create a new tenant with:${NC}"
    echo -e "${BLUE}./create-tenant.sh <tenant_name>${NC}"
    exit 0
fi

echo -e "${GREEN}Found the following tenants:${NC}"
echo ""

printf "%-20s %-40s %-10s\n" "TENANT" "URL" "STATUS"
printf "%-20s %-40s %-10s\n" "------" "---" "------"

while IFS= read -r db_name; do
    if [[ -n "$db_name" ]]; then
        # Extract tenant name (remove golfdb_ prefix)
        tenant_name=${db_name#golfdb_}
        url="https://$tenant_name.golfleaguemanager.app"
        
        # Check if database is accessible
        if docker exec golfleague_postgres psql -U "$PGUSER" -d "$db_name" \
            -c "SELECT 1;" >/dev/null 2>&1; then
            status="✓ Active"
        else
            status="✗ Error"
        fi
        
        printf "%-20s %-40s %-10s\n" "$tenant_name" "$url" "$status"
    fi
done <<< "$tenant_dbs"

echo ""
echo -e "${YELLOW}Commands:${NC}"
echo -e "${BLUE}./create-tenant.sh <name>    # Create new tenant${NC}"
echo -e "${BLUE}./delete-tenant.sh <name>    # Delete tenant${NC}"
echo ""
