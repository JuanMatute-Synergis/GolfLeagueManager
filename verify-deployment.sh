#!/bin/bash

# Deployment verification script for GitHub Actions

echo "🔍 Verifying Golf League Manager deployment..."

# Function to check service health
check_service() {
    local service_name=$1
    local url=$2
    local timeout=${3:-30}
    
    echo "Checking $service_name at $url..."
    
    if timeout ${timeout}s bash -c "until curl -sf $url >/dev/null 2>&1; do sleep 2; done"; then
        echo "✅ $service_name is responding"
        return 0
    else
        echo "❌ $service_name is not responding"
        return 1
    fi
}

# Function to check container status
check_container() {
    local container_name=$1
    
    if docker ps --filter "name=$container_name" --filter "status=running" | grep -q "$container_name"; then
        echo "✅ Container $container_name is running"
        return 0
    else
        echo "❌ Container $container_name is not running"
        return 1
    fi
}

# Main verification
echo ""
echo "📦 Checking container status..."
all_containers_ok=true

for container in golfleague_postgres golfleague_backend golfleague_frontend; do
    if ! check_container "$container"; then
        all_containers_ok=false
    fi
done

echo ""
echo "🌐 Checking service endpoints..."
services_ok=true

# Check database connectivity through backend
if ! check_service "Database (via backend)" "http://localhost:5505/api/health" 60; then
    services_ok=false
fi

# Check backend API
if ! check_service "Backend API" "http://localhost:5505" 30; then
    services_ok=false
fi

# Check frontend
if ! check_service "Frontend" "http://localhost:4500" 30; then
    services_ok=false
fi

echo ""
echo "📊 Deployment Summary:"
echo "Containers: $([ "$all_containers_ok" = true ] && echo "✅ All running" || echo "❌ Issues detected")"
echo "Services: $([ "$services_ok" = true ] && echo "✅ All responding" || echo "❌ Issues detected")"

if [ "$all_containers_ok" = true ] && [ "$services_ok" = true ]; then
    echo ""
    echo "🎉 Deployment verification successful!"
    echo "Frontend: http://localhost:4500"
    echo "Backend: http://localhost:5505"
    exit 0
else
    echo ""
    echo "❌ Deployment verification failed!"
    echo ""
    echo "Container logs for debugging:"
    docker compose logs --tail=10
    exit 1
fi
