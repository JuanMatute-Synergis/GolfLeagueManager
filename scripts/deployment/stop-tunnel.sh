#!/bin/bash

# Stop Golf League Manager Cloudflare Tunnel
echo "Stopping Golf League Manager tunnel..."

# Kill any running cloudflared processes for our tunnel
pkill -f "cloudflared.*golf-league-manager"

echo "Tunnel stopped."
