#!/bin/bash

# Golf League Manager - Development Tenant Launcher
# Usage: ./run-with-tenant.sh [tenant-name]
# Example: ./run-with-tenant.sh htlyons

TENANT=${1:-southmoore}

echo "🏌️  Starting Golf League Manager with tenant: $TENANT"
echo "📁 Database will be: golfdb_$TENANT"
echo ""

# Set the default tenant via command line argument
dotnet run --DefaultTenant="$TENANT"
