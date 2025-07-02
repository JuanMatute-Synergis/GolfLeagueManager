#!/bin/bash

# Golf League Manager Service Control Script

case "$1" in
    start)
        echo "Starting Golf League Manager services..."
        
        # Start Docker containers
        echo "Starting Docker containers..."
        docker-compose up -d
        
        # Start CloudFlared service
        echo "Starting CloudFlared tunnel service..."
        sudo launchctl load /Library/LaunchDaemons/com.cloudflare.cloudflared.plist 2>/dev/null || echo "Service already loaded"
        
        echo "All services started!"
        echo "Application available at: https://htlyons.golfleaguemanager.app"
        ;;
        
    stop)
        echo "Stopping Golf League Manager services..."
        
        # Stop CloudFlared service
        echo "Stopping CloudFlared tunnel service..."
        sudo launchctl unload /Library/LaunchDaemons/com.cloudflare.cloudflared.plist 2>/dev/null || echo "Service already unloaded"
        
        # Stop Docker containers
        echo "Stopping Docker containers..."
        docker-compose down
        
        echo "All services stopped!"
        ;;
        
    restart)
        echo "Restarting Golf League Manager services..."
        $0 stop
        sleep 2
        $0 start
        ;;
        
    status)
        echo "=== Golf League Manager Status ==="
        echo ""
        
        # Check Docker containers
        echo "Docker Containers:"
        if command -v docker-compose &> /dev/null; then
            docker-compose ps
        else
            echo "Docker Compose not found"
        fi
        
        echo ""
        
        # Check CloudFlared service
        echo "CloudFlared Service:"
        if sudo launchctl list | grep -q cloudflared; then
            echo "✅ CloudFlared service is running"
            echo "🌐 Application available at: https://htlyons.golfleaguemanager.app"
        else
            echo "❌ CloudFlared service is NOT running"
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
        ;;
        
    logs)
        echo "=== CloudFlared Service Logs ==="
        echo ""
        echo "Output logs:"
        sudo tail -20 /Library/Logs/com.cloudflare.cloudflared.out.log
        echo ""
        echo "Error logs:"
        sudo tail -20 /Library/Logs/com.cloudflare.cloudflared.err.log
        ;;
        
    *)
        echo "Usage: $0 {start|stop|restart|status|logs}"
        echo ""
        echo "Commands:"
        echo "  start   - Start all services (Docker + CloudFlared tunnel)"
        echo "  stop    - Stop all services"
        echo "  restart - Restart all services"
        echo "  status  - Show status of all services"
        echo "  logs    - Show CloudFlared service logs"
        exit 1
        ;;
esac
