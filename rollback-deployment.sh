#!/bin/bash

# Rollback script for Golf League Manager deployment

echo "🔄 Golf League Manager Rollback Script"
echo ""

# Function to list available backup images
list_backups() {
    echo "Available backup images:"
    echo ""
    echo "Backend backups:"
    docker images | grep "golfleaguemanager.*backend.*backup" || echo "No backend backups found"
    echo ""
    echo "Frontend backups:"
    docker images | grep "golfleaguemanager.*frontend.*backup" || echo "No frontend backups found"
}

# Function to rollback to a specific backup
rollback_to_backup() {
    local backend_backup=$1
    local frontend_backup=$2
    
    if [ -z "$backend_backup" ] || [ -z "$frontend_backup" ]; then
        echo "❌ Both backend and frontend backup tags must be provided"
        echo "Usage: $0 rollback <backend_backup_tag> <frontend_backup_tag>"
        return 1
    fi
    
    echo "🛑 Stopping current containers..."
    docker compose down
    
    echo "🔄 Rolling back to backup images..."
    docker tag "$backend_backup" golfleaguemanager-backend:latest
    docker tag "$frontend_backup" golfleaguemanager-frontend:latest
    
    echo "🚀 Starting rollback deployment..."
    docker compose up -d
    
    echo "⏳ Waiting for services to start..."
    sleep 30
    
    echo "🔍 Verifying rollback..."
    if command -v ./verify-deployment.sh &> /dev/null; then
        chmod +x ./verify-deployment.sh
        ./verify-deployment.sh
    else
        echo "⚠️  Verification script not found, manual verification recommended"
    fi
}

# Function to rollback to the last known good state
quick_rollback() {
    echo "🔄 Attempting quick rollback to most recent backup..."
    
    # Find most recent backup images
    local latest_backend=$(docker images --format "table {{.Repository}}:{{.Tag}}" | grep "golfleaguemanager.*backend.*backup" | head -1)
    local latest_frontend=$(docker images --format "table {{.Repository}}:{{.Tag}}" | grep "golfleaguemanager.*frontend.*backup" | head -1)
    
    if [ -z "$latest_backend" ] || [ -z "$latest_frontend" ]; then
        echo "❌ No recent backup images found"
        echo "Available backups:"
        list_backups
        return 1
    fi
    
    echo "Rolling back to:"
    echo "Backend: $latest_backend"
    echo "Frontend: $latest_frontend"
    
    rollback_to_backup "$latest_backend" "$latest_frontend"
}

# Function to create emergency backup of current state
create_emergency_backup() {
    echo "📦 Creating emergency backup of current state..."
    timestamp=$(date +%Y%m%d_%H%M%S)_emergency
    
    docker tag golfleaguemanager-backend:latest golfleaguemanager-backend:backup_$timestamp 2>/dev/null || echo "⚠️  No current backend image to backup"
    docker tag golfleaguemanager-frontend:latest golfleaguemanager-frontend:backup_$timestamp 2>/dev/null || echo "⚠️  No current frontend image to backup"
    
    echo "✅ Emergency backup created with timestamp: $timestamp"
}

# Function to show help
show_help() {
    echo "Golf League Manager Rollback Script"
    echo ""
    echo "Usage:"
    echo "  $0 list                                    - List available backup images"
    echo "  $0 quick                                   - Quick rollback to most recent backup"
    echo "  $0 rollback <backend_tag> <frontend_tag>  - Rollback to specific backup"
    echo "  $0 backup                                  - Create emergency backup of current state"
    echo "  $0 help                                    - Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 quick"
    echo "  $0 rollback golfleaguemanager-backend:backup_20250701_140000 golfleaguemanager-frontend:backup_20250701_140000"
}

# Main script logic
case "$1" in
    "list")
        list_backups
        ;;
    "quick")
        quick_rollback
        ;;
    "rollback")
        rollback_to_backup "$2" "$3"
        ;;
    "backup")
        create_emergency_backup
        ;;
    "help"|"--help"|"-h")
        show_help
        ;;
    *)
        echo "❌ Unknown command: $1"
        echo ""
        show_help
        exit 1
        ;;
esac
