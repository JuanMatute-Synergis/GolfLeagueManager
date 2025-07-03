#!/bin/bash

# Start Golf League Manager Cloudflare Tunnel
# This script starts the cloudflared tunnel for the Golf League Manager application

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CONFIG_FILE="$SCRIPT_DIR/cloudflared-config.yml"

echo "Starting Golf League Manager tunnel..."
echo "Configuration: $CONFIG_FILE"
echo "Domain: htlyons.golfleaguemanager.app"
echo ""

# Check if Docker containers are running
if ! docker-compose -f "$SCRIPT_DIR/docker-compose.yml" ps | grep -q "Up"; then
    echo "Warning: Docker containers don't appear to be running."
    echo "Starting Docker containers..."
    docker-compose -f "$SCRIPT_DIR/docker-compose.yml" up -d
    echo "Waiting 10 seconds for containers to start..."
    sleep 10
fi

# Start the tunnel
cloudflared tunnel --config "$CONFIG_FILE" run golf-league-manager
