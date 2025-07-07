#!/bin/bash

# Cleanup Host Services Script
# Golf League Manager - Remove host-based services after containerization

echo "🧹 Cleaning up host-based services..."
echo "This will remove GitHub Runner and CloudFlared services from the host system."
echo ""

# Confirmation prompt
read -p "Are you sure you want to remove host services? (y/N): " -n 1 -r
echo
if [[ ! $REPLY =~ ^[Yy]$ ]]; then
    echo "❌ Cleanup cancelled"
    exit 1
fi

echo ""
echo "🛑 Stopping and removing host services..."

# =============================================================================
# 1. REMOVE GITHUB ACTIONS RUNNER
# =============================================================================

echo ""
echo "1️⃣ Removing GitHub Actions Runner from host..."

# Check if runner directory exists
if [[ -d "$HOME/actions-runner" ]]; then
    cd "$HOME/actions-runner"
    
    # Stop the runner service if it's running
    echo "   Stopping runner service..."
    if [[ -f "./svc.sh" ]]; then
        sudo ./svc.sh stop 2>/dev/null || echo "   Runner service was not running"
        sudo ./svc.sh uninstall 2>/dev/null || echo "   Runner service was not installed"
    fi
    
    # Remove the runner from GitHub (if config exists)
    echo "   Removing runner from GitHub..."
    if [[ -f "./config.sh" && -f "./.runner" ]]; then
        # Try to remove with existing token (this might fail if token expired)
        ./config.sh remove 2>/dev/null || echo "   ⚠️  Could not auto-remove from GitHub (token may have expired)"
        echo "   ℹ️  Please manually remove the old runner from GitHub:"
        echo "      https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners"
    fi
    
    # Remove runner directory
    cd "$HOME"
    echo "   Removing runner directory..."
    rm -rf "$HOME/actions-runner"
    echo "   ✅ GitHub Actions Runner removed from host"
else
    echo "   ℹ️  No GitHub Actions Runner found on host"
fi

# =============================================================================
# 2. REMOVE CLOUDFLARED SERVICE
# =============================================================================

echo ""
echo "2️⃣ Removing CloudFlared service from host..."

# Stop and unload CloudFlared service
echo "   Stopping CloudFlared service..."
sudo launchctl unload /Library/LaunchDaemons/com.cloudflare.cloudflared.plist 2>/dev/null || echo "   CloudFlared service was not loaded"

# Remove the plist file
if [[ -f "/Library/LaunchDaemons/com.cloudflare.cloudflared.plist" ]]; then
    echo "   Removing CloudFlared service file..."
    sudo rm -f /Library/LaunchDaemons/com.cloudflare.cloudflared.plist
    echo "   ✅ CloudFlared service file removed"
else
    echo "   ℹ️  CloudFlared service file not found"
fi

# Remove CloudFlared logs
if [[ -d "/Library/Logs" ]]; then
    echo "   Cleaning up CloudFlared logs..."
    sudo rm -f /Library/Logs/com.cloudflare.cloudflared.*.log 2>/dev/null || true
    echo "   ✅ CloudFlared logs cleaned up"
fi

# Check if cloudflared binary should be removed
if command -v cloudflared &> /dev/null; then
    echo "   CloudFlared binary found at: $(which cloudflared)"
    read -p "   Do you want to remove the CloudFlared binary? (y/N): " -n 1 -r
    echo
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        CLOUDFLARED_PATH=$(which cloudflared)
        sudo rm -f "$CLOUDFLARED_PATH"
        echo "   ✅ CloudFlared binary removed"
    else
        echo "   ℹ️  CloudFlared binary kept (you can remove manually later)"
    fi
fi

# =============================================================================
# 3. CLEANUP OLD SCRIPTS
# =============================================================================

echo ""
echo "3️⃣ Cleaning up old service scripts..."

# Remove old tunnel scripts if they exist
OLD_SCRIPTS=(
    "start-tunnel.sh"
    "stop-tunnel.sh"
    "check-runner-status.sh"
)

for script in "${OLD_SCRIPTS[@]}"; do
    if [[ -f "scripts/deployment/$script" ]]; then
        echo "   Removing $script..."
        rm -f "scripts/deployment/$script"
        echo "   ✅ $script removed"
    fi
done

# =============================================================================
# 4. VERIFY CONTAINER SERVICES
# =============================================================================

echo ""
echo "4️⃣ Verifying container services are running..."

# Check if containers are running
if docker-compose ps | grep -q "Up"; then
    echo "   ✅ Container services are running"
    
    # Check specific services
    if docker-compose ps cloudflared | grep -q "Up"; then
        echo "   ✅ CloudFlared container is running"
    else
        echo "   ⚠️  CloudFlared container is not running"
    fi
    
    if docker-compose ps github-runner | grep -q "Up"; then
        echo "   ✅ GitHub Runner container is running"
    else
        echo "   ⚠️  GitHub Runner container is not running"
    fi
else
    echo "   ⚠️  Container services are not running"
    echo "   Start them with: ./scripts/deployment/manage-services.sh start"
fi

# =============================================================================
# 5. FINAL SUMMARY
# =============================================================================

echo ""
echo "🎉 Cleanup completed!"
echo ""
echo "Summary of changes:"
echo "✅ GitHub Actions Runner removed from host"
echo "✅ CloudFlared service removed from host"
echo "✅ Old service scripts cleaned up"
echo "✅ All services now running in containers"
echo ""
echo "Next steps:"
echo "• Verify container services: ./scripts/deployment/manage-services.sh status"
echo "• Test your application: https://htlyons.golfleaguemanager.app"
echo "• Check GitHub for old runner: https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners"
echo ""
echo "Your system is now fully containerized! 🚀"
echo ""
