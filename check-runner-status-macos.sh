#!/bin/bash

# GitHub Actions Runner Status Checker for macOS
echo "🏃‍♂️ GitHub Actions Runner Status Check"
echo "========================================"

# Check if runner service is running
echo "📋 Runner Service Status:"
cd /Users/juanmatute/Projects/actions-runner 2>/dev/null && ./svc.sh status || echo "❌ Could not access runner directory"

echo ""
echo "📊 Recent Runner Activity (last 10 lines):"
tail -10 /Users/juanmatute/Library/Logs/actions.runner.JuanMatute-Synergis-GolfLeagueManager.mac-mini-runner/stdout.log 2>/dev/null || echo "No logs available"

echo ""
echo "🐳 Docker Status:"
echo "Running containers:"
docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}" 2>/dev/null || echo "Docker not available"

echo ""
echo "📦 Available Docker Images:"
docker images | grep -E "(golfleague|postgres)" | head -5 2>/dev/null || echo "No Docker images found"

echo ""
echo "🔧 System Resources:"
echo "Disk usage:"
df -h / | tail -1 2>/dev/null || echo "Could not get disk usage"

echo ""
echo "🌐 Service Health Check:"
echo "Checking backend API..."
if curl -s http://localhost:5505/api/health >/dev/null 2>&1; then
    echo "✅ Backend API is responding"
    curl -s http://localhost:5505/api/health 2>/dev/null || echo "Response received but not readable"
else
    echo "❌ Backend API is not responding"
fi

echo "Checking frontend..."
if curl -s http://localhost:4500 >/dev/null 2>&1; then
    echo "✅ Frontend is responding"
else
    echo "❌ Frontend is not responding"
fi

echo ""
echo "📝 Quick Actions:"
echo "• To restart runner: cd /Users/juanmatute/Projects/actions-runner && ./svc.sh restart"
echo "• To check deployment: ./verify-deployment.sh"
echo "• To view full logs: tail -f /Users/juanmatute/Library/Logs/actions.runner.*/stdout.log"
echo "• To trigger manual deployment: Go to GitHub → Actions → Manual Deployment"
