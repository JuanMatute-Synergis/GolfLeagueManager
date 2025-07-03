#!/bin/bash

# Migrate all existing tenant databases
# This script applies Entity Framework migrations to all tenant databases

echo "🔄 Migrating all tenant databases..."

# Get list of tenant databases
TENANT_DBS=$(docker exec golfleague_postgres psql -U golfuser -d postgres -t -c "SELECT datname FROM pg_database WHERE datname LIKE 'golfdb_%';" | sed 's/^[ \t]*//' | sed 's/[ \t]*$//')

if [ -z "$TENANT_DBS" ]; then
    echo "❌ No tenant databases found"
    exit 1
fi

echo "Found tenant databases:"
echo "$TENANT_DBS"

# Run migrations for each tenant database
cd backend

# Ensure the project is restored first
echo "🔄 Restoring .NET project..."
dotnet restore

while IFS= read -r db; do
    if [ "$db" != "" ]; then
        tenant_id=${db#golfdb_}
        echo "🔄 Migrating tenant: $tenant_id (database: $db)"
        
        # Temporarily set connection string for this tenant
        export ConnectionStrings__DefaultConnection="Host=localhost;Database=$db;Username=golfuser;Password=golfpassword"
        
        # Run migrations (remove --no-build flag to allow building)
        dotnet ef database update
        
        if [ $? -eq 0 ]; then
            echo "✅ Successfully migrated $tenant_id"
        else
            echo "❌ Failed to migrate $tenant_id"
        fi
        
        echo "---"
    fi
done <<< "$TENANT_DBS"

echo "🎉 Migration process completed!"
