#!/bin/bash

# Golf League Manager Health Check Script

echo "=== Golf League Manager Health Check ==="
echo ""

# Check if application is accessible
echo "🌐 Testing external access..."
HTTP_CODE=$(curl -s -o /dev/null -w "%{http_code}" https://htlyons.golfleaguemanager.app)
if [ "$HTTP_CODE" = "200" ]; then
    echo "✅ Application is accessible at https://htlyons.golfleaguemanager.app"
else
    echo "❌ Application is NOT accessible (HTTP $HTTP_CODE)"
fi

echo ""

# Check Docker containers
echo "🐳 Checking Docker containers..."
if docker-compose ps | grep -q "Up"; then
    echo "✅ Docker containers are running"
else
    echo "❌ Docker containers are NOT running"
fi

echo ""

# Check CloudFlared service
echo "☁️  Checking CloudFlared service..."
if sudo launchctl list | grep -q cloudflared; then
    echo "✅ CloudFlared service is running"
else
    echo "❌ CloudFlared service is NOT running"
fi

echo ""

# Check local services
echo "🔧 Checking local services..."
if lsof -i :4500 &> /dev/null; then
    echo "✅ Frontend service (port 4500) is running"
else
    echo "❌ Frontend service (port 4500) is NOT running"
fi

if lsof -i :5505 &> /dev/null; then
    echo "✅ Backend service (port 5505) is running"
else
    echo "❌ Backend service (port 5505) is NOT running"
fi

if lsof -i :5432 &> /dev/null; then
    echo "✅ Database service (port 5432) is running"
else
    echo "❌ Database service (port 5432) is NOT running"
fi

echo ""
echo "=== Health Check Complete ==="
