#!/bin/bash

# Golf League Manager Health Check Script
# This script performs comprehensive health checks on the application

echo "🏥 Golf League Manager Health Check"
echo "====================================="
echo ""

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Health check results
HEALTH_STATUS="HEALTHY"
ISSUES_FOUND=0

# Function to log health status
log_status() {
    local service=$1
    local status=$2
    local message=$3
    
    if [ "$status" = "OK" ]; then
        echo -e "✅ ${service}: ${GREEN}${status}${NC} - $message"
    elif [ "$status" = "WARNING" ]; then
        echo -e "⚠️  ${service}: ${YELLOW}${status}${NC} - $message"
        if [ "$HEALTH_STATUS" = "HEALTHY" ]; then
            HEALTH_STATUS="WARNING"
        fi
    else
        echo -e "❌ ${service}: ${RED}${status}${NC} - $message"
        HEALTH_STATUS="UNHEALTHY"
        ISSUES_FOUND=$((ISSUES_FOUND + 1))
    fi
}

echo "🔍 Checking Docker containers..."
echo "--------------------------------"

# Check if Docker is running
if ! docker info > /dev/null 2>&1; then
    log_status "Docker" "CRITICAL" "Docker daemon is not running"
    exit 1
fi

# Check PostgreSQL container
if docker ps --format "table {{.Names}}" | grep -q "golfleague_postgres"; then
    if docker exec golfleague_postgres pg_isready -U golfuser > /dev/null 2>&1; then
        log_status "PostgreSQL" "OK" "Database is running and accepting connections"
    else
        log_status "PostgreSQL" "CRITICAL" "Database is not accepting connections"
    fi
else
    log_status "PostgreSQL" "CRITICAL" "Database container is not running"
fi

# Check Backend container
if docker ps --format "table {{.Names}}" | grep -q "golfleague_backend"; then
    if curl -s http://localhost:5505/health > /dev/null 2>&1; then
        log_status "Backend" "OK" "API is responding on port 5505"
    else
        log_status "Backend" "WARNING" "API health check failed, but container is running"
    fi
else
    log_status "Backend" "CRITICAL" "Backend container is not running"
fi

# Check Frontend container
if docker ps --format "table {{.Names}}" | grep -q "golfleague_frontend"; then
    if curl -s http://localhost:4500 > /dev/null 2>&1; then
        log_status "Frontend" "OK" "Frontend is responding on port 4500"
    else
        log_status "Frontend" "WARNING" "Frontend health check failed, but container is running"
    fi
else
    log_status "Frontend" "CRITICAL" "Frontend container is not running"
fi

# Check CloudFlared container (optional)
if docker ps --format "table {{.Names}}" | grep -q "golfleague_cloudflared"; then
    log_status "CloudFlared" "OK" "CloudFlared tunnel is running"
else
    log_status "CloudFlared" "WARNING" "CloudFlared container is not running (optional service)"
fi

echo ""
echo "🗄️ Checking databases..."
echo "-------------------------"

# Check tenant databases
if docker exec golfleague_postgres psql -U golfuser -d postgres -t -c "SELECT COUNT(*) FROM pg_database WHERE datname LIKE 'golfdb_%';" > /dev/null 2>&1; then
    DB_COUNT=$(docker exec golfleague_postgres psql -U golfuser -d postgres -t -c "SELECT COUNT(*) FROM pg_database WHERE datname LIKE 'golfdb_%';" | tr -d ' ')
    if [ "$DB_COUNT" -gt 0 ]; then
        log_status "Tenant Databases" "OK" "Found $DB_COUNT tenant database(s)"
    else
        log_status "Tenant Databases" "WARNING" "No tenant databases found"
    fi
else
    log_status "Tenant Databases" "CRITICAL" "Cannot check tenant databases"
fi

echo ""
echo "💾 Checking system resources..."
echo "-------------------------------"

# Check disk space
DISK_USAGE=$(df / | awk 'NR==2 {print $5}' | sed 's/%//')
if [ "$DISK_USAGE" -lt 80 ]; then
    log_status "Disk Space" "OK" "Disk usage: ${DISK_USAGE}%"
elif [ "$DISK_USAGE" -lt 90 ]; then
    log_status "Disk Space" "WARNING" "Disk usage: ${DISK_USAGE}% - Consider cleanup"
else
    log_status "Disk Space" "CRITICAL" "Disk usage: ${DISK_USAGE}% - Immediate attention required"
fi

# Check memory usage
if command -v free > /dev/null 2>&1; then
    MEM_USAGE=$(free | awk 'NR==2{printf "%.0f", $3*100/$2}')
    if [ "$MEM_USAGE" -lt 80 ]; then
        log_status "Memory" "OK" "Memory usage: ${MEM_USAGE}%"
    elif [ "$MEM_USAGE" -lt 90 ]; then
        log_status "Memory" "WARNING" "Memory usage: ${MEM_USAGE}%"
    else
        log_status "Memory" "CRITICAL" "Memory usage: ${MEM_USAGE}%"
    fi
else
    log_status "Memory" "OK" "Memory check not available on this system"
fi

# Check Docker resource usage
echo ""
echo "🐳 Docker resource usage:"
docker stats --no-stream --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}\t{{.MemPerc}}" 2>/dev/null || echo "Docker stats not available"

echo ""
echo "📊 Summary"
echo "----------"
echo "Health Status: $HEALTH_STATUS"
echo "Issues Found: $ISSUES_FOUND"
echo "Timestamp: $(date)"

# Exit with appropriate code
if [ "$HEALTH_STATUS" = "HEALTHY" ]; then
    echo -e "${GREEN}✅ All systems are healthy!${NC}"
    exit 0
elif [ "$HEALTH_STATUS" = "WARNING" ]; then
    echo -e "${YELLOW}⚠️  System has warnings but is operational${NC}"
    exit 0
else
    echo -e "${RED}❌ System has critical issues requiring attention${NC}"
    exit 1
fi
