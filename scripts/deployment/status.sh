#!/bin/bash

# Check Golf League Manager status
echo "=== Golf League Manager Status ==="
echo ""

# Check Docker containers
echo "Docker Containers:"
if command -v docker-compose &> /dev/null; then
    cd "$(dirname "${BASH_SOURCE[0]}")"
    docker-compose ps
else
    echo "Docker Compose not found"
fi

echo ""

# Check CloudFlared tunnel
echo "CloudFlared Tunnel:"
if pgrep -f "cloudflared.*golf-league-manager" > /dev/null; then
    echo "✅ CloudFlared tunnel is running"
    echo "🌐 Application available at: https://htlyons.golfleaguemanager.app"
else
    echo "❌ CloudFlared tunnel is NOT running"
fi

echo ""

# Check local ports
echo "Local Services:"
if lsof -i :4500 &> /dev/null; then
    echo "✅ Frontend running on port 4500"
else
    echo "❌ Frontend NOT running on port 4500"
fi

if lsof -i :5505 &> /dev/null; then
    echo "✅ Backend running on port 5505"
else
    echo "❌ Backend NOT running on port 5505"
fi

if lsof -i :5432 &> /dev/null; then
    echo "✅ Database running on port 5432"
else
    echo "❌ Database NOT running on port 5432"
fi
