#!/bin/bash

# Golf League Manager - Docker I/O Error Fix Script
# For ARM64 macOS Docker Desktop issues

echo "=== Docker I/O Error Resolution ==="
echo ""

echo "1. Checking Docker Desktop status..."
if ! docker info >/dev/null 2>&1; then
    echo "❌ Docker Desktop is not running"
    echo "   Starting Docker Desktop..."
    open -a "Docker Desktop"
    echo "   ⏳ Please wait 30-60 seconds for Docker to start"
    exit 1
fi

echo "2. Checking disk space..."
DISK_USAGE=$(df / | tail -1 | awk '{print $5}' | sed 's/%//')
if [ "$DISK_USAGE" -gt 85 ]; then
    echo "⚠️  Disk usage is ${DISK_USAGE}% - this may cause I/O errors"
    echo "   Consider freeing up disk space"
fi

echo "3. Cleaning Docker cache..."
docker system prune -f
docker builder prune -f

echo "4. Checking Docker settings..."
echo "   Current Docker info:"
docker info | grep -E "(Server Version|Storage Driver|Docker Root Dir|Total Memory)"

echo ""
echo "5. Suggested Docker Desktop settings for ARM64 macOS:"
echo "   💾 Memory: 8GB or higher"
echo "   💿 Disk: 100GB or higher"
echo "   🔧 Storage Driver: overlay2"
echo "   📁 Use virtualization framework: VirtioFS"

echo ""
echo "6. Alternative approaches:"
echo "   • Use native development: ./manage-services.sh dev-native"
echo "   • Build images on different machine"
echo "   • Use pre-built images from registry"

echo ""
echo "✅ If Docker is running, try building again with:"
echo "   docker-compose build --no-cache --parallel"
