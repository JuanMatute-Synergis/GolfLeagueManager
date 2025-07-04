#!/bin/bash

# Migrate all existing tenant databases
# This script applies Entity Framework migrations to all tenant databases

echo "🔄 Migrating all tenant databases..."

# Check if we're in the correct directory
if [ ! -f "backend/backend.csproj" ]; then
    echo "❌ Error: This script must be run from the project root directory"
    echo "   Current directory: $(pwd)"
    echo "   Expected: Directory containing backend/backend.csproj"
    exit 1
fi

# Check if Docker is running and database is accessible
echo "🔍 Checking database connectivity..."
if ! docker exec golfleague_postgres psql -U golfuser -d postgres -c "SELECT 1;" > /dev/null 2>&1; then
    echo "❌ Error: Cannot connect to PostgreSQL database"
    echo "   Make sure Docker is running and the database container is up"
    echo "   Run: docker-compose up -d"
    exit 1
fi

# Get list of tenant databases
TENANT_DBS=$(docker exec golfleague_postgres psql -U golfuser -d postgres -t -c "SELECT datname FROM pg_database WHERE datname LIKE 'golfdb_%';" 2>/dev/null | sed 's/^[ \t]*//' | sed 's/[ \t]*$//' | grep -v '^$')

if [ -z "$TENANT_DBS" ]; then
    echo "⚠️  No tenant databases found - this might be a fresh installation"
    echo "Creating default tenant database..."
    docker exec golfleague_postgres psql -U golfuser -d postgres -c "CREATE DATABASE golfdb_default;" 2>/dev/null || true
    TENANT_DBS="golfdb_default"
fi

echo "Found tenant databases:"
echo "$TENANT_DBS"
echo ""

# Change to backend directory
cd backend

# Ensure the project is restored first
echo "🔄 Restoring .NET project..."
if ! dotnet restore; then
    echo "❌ Failed to restore .NET project"
    exit 1
fi

# Store original connection string environment variable if it exists
ORIGINAL_MIGRATION_CONNECTION_STRING="$MIGRATION_CONNECTION_STRING"

# Counter for success/failure tracking
MIGRATED_COUNT=0
FAILED_COUNT=0

# Run migrations for each tenant database
while IFS= read -r db; do
    if [ "$db" != "" ]; then
        tenant_id=${db#golfdb_}
        echo "🔄 Migrating tenant: $tenant_id (database: $db)"
        
        # Set the migration connection string environment variable
        export MIGRATION_CONNECTION_STRING="Host=localhost;Port=5432;Database=$db;Username=golfuser;Password=golfpassword"
        
        # Run migrations
        if dotnet ef database update --verbose 2>&1; then
            echo "✅ Successfully migrated $tenant_id"
            MIGRATED_COUNT=$((MIGRATED_COUNT + 1))
        else
            echo "❌ Failed to migrate $tenant_id"
            FAILED_COUNT=$((FAILED_COUNT + 1))
            echo "⚠️  Continuing with other tenants..."
        fi
        
        echo "---"
    fi
done <<< "$TENANT_DBS"

# Restore original environment variable
if [ -n "$ORIGINAL_MIGRATION_CONNECTION_STRING" ]; then
    export MIGRATION_CONNECTION_STRING="$ORIGINAL_MIGRATION_CONNECTION_STRING"
else
    unset MIGRATION_CONNECTION_STRING
fi

echo ""
echo "🎉 Migration process completed!"
echo "📊 Summary:"
echo "   ✅ Successfully migrated: $MIGRATED_COUNT tenant(s)"
echo "   ❌ Failed migrations: $FAILED_COUNT tenant(s)"

if [ $FAILED_COUNT -gt 0 ]; then
    echo ""
    echo "⚠️  Some migrations failed. Please check the output above for details."
    exit 1
fi
