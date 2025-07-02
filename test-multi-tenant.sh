#!/bin/bash

# Multi-Tenant Golf League Manager Test Script
# This script demonstrates the multi-tenant functionality

echo "🏌️ Golf League Manager Multi-Tenant Test"
echo "=========================================="

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
RED='\033[0;31m'
NC='\033[0m' # No Color

echo -e "${BLUE}1. Testing Main Application Access${NC}"
echo "Testing htlyons.golfleaguemanager.app..."
HTLYONS_STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://htlyons.golfleaguemanager.app)
if [ "$HTLYONS_STATUS" -eq 200 ]; then
    echo -e "${GREEN}✅ htlyons.golfleaguemanager.app is accessible (HTTP $HTLYONS_STATUS)${NC}"
else
    echo -e "${RED}❌ htlyons.golfleaguemanager.app failed (HTTP $HTLYONS_STATUS)${NC}"
fi

echo ""
echo -e "${BLUE}2. Testing New Tenant Subdomain${NC}"
echo "Testing testclient.golfleaguemanager.app..."
TESTCLIENT_STATUS=$(curl -s -o /dev/null -w "%{http_code}" https://testclient.golfleaguemanager.app)
if [ "$TESTCLIENT_STATUS" -eq 200 ]; then
    echo -e "${GREEN}✅ testclient.golfleaguemanager.app is accessible (HTTP $TESTCLIENT_STATUS)${NC}"
else
    echo -e "${RED}❌ testclient.golfleaguemanager.app failed (HTTP $TESTCLIENT_STATUS)${NC}"
fi

echo ""
echo -e "${BLUE}3. Testing Backend Health${NC}"
BACKEND_HEALTH=$(curl -s http://localhost:5505/health)
if [[ "$BACKEND_HEALTH" == *"Healthy"* ]]; then
    echo -e "${GREEN}✅ Backend is healthy: $BACKEND_HEALTH${NC}"
else
    echo -e "${RED}❌ Backend health check failed${NC}"
fi

echo ""
echo -e "${BLUE}4. Testing Database Connections${NC}"
echo "Checking tenant databases..."

# Connect to PostgreSQL and list databases
DATABASES=$(docker exec golfleague_postgres psql -U golfuser -d postgres -t -c "SELECT datname FROM pg_database WHERE datname LIKE 'golfdb%';" 2>/dev/null | sed 's/^ *//' | grep -v '^$')

if [ -n "$DATABASES" ]; then
    echo -e "${GREEN}✅ Found tenant databases:${NC}"
    echo "$DATABASES" | while read -r db; do
        echo "  📁 $db"
    done
else
    echo -e "${YELLOW}⚠️  No tenant databases found yet (they will be created on first access)${NC}"
fi

echo ""
echo -e "${BLUE}5. Testing Service Status${NC}"
DOCKER_STATUS=$(docker-compose ps --services --filter "status=running" 2>/dev/null | wc -l)
echo -e "${GREEN}✅ $DOCKER_STATUS Docker services running${NC}"

CLOUDFLARED_STATUS=$(launchctl list | grep cloudflared)
if [ -n "$CLOUDFLARED_STATUS" ]; then
    echo -e "${GREEN}✅ CloudFlared service is running${NC}"
else
    echo -e "${RED}❌ CloudFlared service not found${NC}"
fi

echo ""
echo -e "${BLUE}6. Testing Wildcard Subdomain Support${NC}"
echo "You can test any subdomain by visiting:"
echo -e "${YELLOW}  https://anytenant.golfleaguemanager.app${NC}"
echo -e "${YELLOW}  https://client123.golfleaguemanager.app${NC}"
echo -e "${YELLOW}  https://demo.golfleaguemanager.app${NC}"

echo ""
echo -e "${GREEN}🎉 Multi-tenant setup test completed!${NC}"
echo ""
echo -e "${BLUE}📋 Summary:${NC}"
echo "• Each subdomain gets its own database (golfdb_{tenant})"
echo "• Databases are created automatically on first access"
echo "• All data is isolated between tenants"
echo "• Same application code serves all tenants"
echo ""
echo -e "${BLUE}🔗 Access your tenants:${NC}"
echo "• Main: https://htlyons.golfleaguemanager.app"
echo "• Test: https://testclient.golfleaguemanager.app"
echo "• Custom: https://{your-tenant}.golfleaguemanager.app"
