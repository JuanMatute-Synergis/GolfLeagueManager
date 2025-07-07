#!/bin/bash

# Migration script: Move CloudFlared from host service to Docker container
# Golf League Manager - CloudFlared Container Migration

echo "🚀 Migrating CloudFlared from host service to Docker container..."
echo ""

# Step 1: Stop the current host-based CloudFlared service
echo "1. Stopping current CloudFlared host service..."
if sudo launchctl list | grep -q cloudflared; then
    echo "   Stopping CloudFlared service..."
    sudo launchctl unload /Library/LaunchDaemons/com.cloudflare.cloudflared.plist 2>/dev/null || echo "   Service already unloaded"
    echo "   ✅ CloudFlared host service stopped"
else
    echo "   ℹ️  CloudFlared host service was not running"
fi

echo ""

# Step 2: Stop current Docker containers
echo "2. Stopping current Docker containers..."
docker-compose down
echo "   ✅ Docker containers stopped"

echo ""

# Step 3: Verify credentials file exists
echo "3. Verifying CloudFlared credentials..."
CREDENTIALS_FILE="$HOME/.cloudflared/64746180-e8db-4616-abfb-0433703b2645.json"
if [[ -f "$CREDENTIALS_FILE" ]]; then
    echo "   ✅ Credentials file found: $CREDENTIALS_FILE"
else
    echo "   ❌ ERROR: Credentials file not found: $CREDENTIALS_FILE"
    echo "   Please ensure the CloudFlared credentials file exists before continuing."
    exit 1
fi

echo ""

# Step 4: Start with new containerized setup
echo "4. Starting new containerized setup..."
docker-compose up -d
echo "   ✅ All containers started (including CloudFlared)"

echo ""

# Step 5: Wait for services to be ready
echo "5. Waiting for services to initialize..."
sleep 10

echo ""

# Step 6: Verify everything is working
echo "6. Verifying deployment..."

# Check Docker containers
echo "   Checking containers:"
if docker-compose ps | grep -q "Up"; then
    echo "   ✅ Docker containers are running"
else
    echo "   ❌ Some Docker containers are not running"
fi

# Check CloudFlared specifically
if docker-compose ps cloudflared | grep -q "Up"; then
    echo "   ✅ CloudFlared container is running"
else
    echo "   ❌ CloudFlared container is not running"
fi

echo ""

# Step 7: Test connectivity
echo "7. Testing connectivity..."
echo "   🌐 Your application should be available at: https://htlyons.golfleaguemanager.app"
echo "   Please test the application in your browser to confirm everything is working."

echo ""
echo "🎉 Migration completed!"
echo ""
echo "Summary of changes:"
echo "✅ CloudFlared now runs in a Docker container"
echo "✅ All services managed through Docker Compose"
echo "✅ Simplified service management with ./manage-services.sh"
echo "✅ Better isolation and consistency"
echo ""
echo "Next steps:"
echo "• Test your application at https://htlyons.golfleaguemanager.app"
echo "• Use './manage-services.sh status' to check service health"
echo "• Use './manage-services.sh logs' to view CloudFlared logs"
echo ""
echo "If you encounter any issues, you can view logs with:"
echo "  docker-compose logs cloudflared"
echo ""
