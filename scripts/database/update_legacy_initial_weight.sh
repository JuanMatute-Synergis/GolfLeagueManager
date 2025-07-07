#!/bin/bash

# Script to update LegacyInitialWeight from 4 to 1 for all tenant databases
# This corrects the default value based on user clarification

set -e

# Database connection settings
DB_HOST="localhost"
DB_PORT="5432"
DB_USER="golfuser"
DB_PASSWORD="GolfPass123!"

# SQL script path
SQL_SCRIPT="/app/scripts/database/update_legacy_initial_weight.sql"

echo "Starting LegacyInitialWeight update for all tenant databases..."

# Get list of tenant databases
TENANT_DBS=$(docker exec golfleague_postgres psql -U $DB_USER -d postgres -t -c "SELECT datname FROM pg_database WHERE datname LIKE 'golfdb_%';" | tr -d ' ')

# Process each tenant database
for db in $TENANT_DBS; do
    if [ -n "$db" ]; then
        echo "Updating database: $db"
        
        # Check current values before update
        echo "Current LegacyInitialWeight values in $db:"
        docker exec golfleague_postgres psql -U $DB_USER -d $db -c "SELECT \"Id\", \"TenantId\", \"AverageMethod\", \"LegacyInitialWeight\" FROM \"LeagueSettings\";"
        
        # Apply the update
        echo "Applying update to $db..."
        docker exec golfleague_postgres psql -U $DB_USER -d $db -f $SQL_SCRIPT
        
        echo "Update completed for $db"
        echo "----------------------------------------"
    fi
done

echo "LegacyInitialWeight update completed for all tenant databases."
