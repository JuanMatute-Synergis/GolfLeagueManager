#!/bin/bash

# Golf League Manager Service Control Script

case "$1" in
    start)
        echo "Starting Golf League Manager services..."
        
        # Start all Docker containers (including CloudFlared)
        echo "Starting all Docker containers..."
        docker-compose up -d
        
        echo "All services started!"
        echo "Application available at: https://htlyons.golfleaguemanager.app"
        ;;
        
    start-app)
        echo "Starting Golf League Manager application services..."
        echo "(Skipping GitHub Runner container - using native runner instead)"
        
        # Start core application containers only
        echo "Starting core application containers..."
        docker-compose up -d postgres backend frontend cloudflared
        
        echo "Application services started!"
        echo "Application available at: https://htlyons.golfleaguemanager.app"
        echo ""
        echo "Note: Using native GitHub runner instead of container"
        ;;
        
    dev-native)
        echo "=== Starting Development Mode (Native) ==="
        echo ""
        echo "🚀 Starting Golf League Manager in native development mode"
        echo "   (No Docker containers - running services directly)"
        echo ""
        
        # Check if .NET is installed
        if ! command -v dotnet &> /dev/null; then
            echo "❌ .NET SDK not found. Please install .NET 8 SDK:"
            echo "   https://dotnet.microsoft.com/download/dotnet/8.0"
            exit 1
        fi
        
        # Check if Node.js is installed
        if ! command -v node &> /dev/null; then
            echo "❌ Node.js not found. Please install Node.js:"
            echo "   https://nodejs.org/"
            exit 1
        fi
        
        echo "Starting PostgreSQL via Docker (lightweight)..."
        docker run -d --name golfleague_postgres_dev \
            -e POSTGRES_USER=golfuser \
            -e POSTGRES_PASSWORD=golfpassword \
            -e POSTGRES_DB=golfdb \
            -p 5432:5432 \
            postgres:16 2>/dev/null || echo "PostgreSQL container already running"
        
        echo "Starting backend (native .NET)..."
        cd backend
        nohup dotnet run > ../backend.log 2>&1 &
        BACKEND_PID=$!
        echo $BACKEND_PID > ../backend.pid
        cd ..
        
        echo "Installing frontend dependencies..."
        cd frontend
        if [ ! -d "node_modules" ]; then
            npm install
        fi
        
        echo "Starting frontend (native Node.js)..."
        nohup npm start > ../frontend.log 2>&1 &
        FRONTEND_PID=$!
        echo $FRONTEND_PID > ../frontend.pid
        cd ..
        
        sleep 3
        
        echo ""
        echo "✅ Development environment started!"
        echo "📱 Frontend: http://localhost:4200"
        echo "🔗 Backend:  http://localhost:5505"
        echo "🗄️  Database: localhost:5432"
        echo ""
        echo "📋 Process IDs saved to:"
        echo "   Backend:  backend.pid"
        echo "   Frontend: frontend.pid"
        echo ""
        echo "📝 Logs available at:"
        echo "   Backend:  backend.log"
        echo "   Frontend: frontend.log"
        ;;
        
    stop)
        echo "Stopping Golf League Manager services..."
        
        # Stop all Docker containers (including CloudFlared)
        echo "Stopping all Docker containers..."
        docker-compose down
        
        echo "All services stopped!"
        ;;
        
    stop-dev)
        echo "=== Stopping Development Mode (Native) ==="
        echo ""
        
        # Stop backend
        if [ -f "backend.pid" ]; then
            BACKEND_PID=$(cat backend.pid)
            if ps -p $BACKEND_PID > /dev/null 2>&1; then
                echo "Stopping backend (PID: $BACKEND_PID)..."
                kill $BACKEND_PID
            fi
            rm backend.pid
        fi
        
        # Stop frontend
        if [ -f "frontend.pid" ]; then
            FRONTEND_PID=$(cat frontend.pid)
            if ps -p $FRONTEND_PID > /dev/null 2>&1; then
                echo "Stopping frontend (PID: $FRONTEND_PID)..."
                kill $FRONTEND_PID
            fi
            rm frontend.pid
        fi
        
        # Stop PostgreSQL container
        echo "Stopping PostgreSQL container..."
        docker stop golfleague_postgres_dev 2>/dev/null || echo "PostgreSQL container not running"
        docker rm golfleague_postgres_dev 2>/dev/null || echo "PostgreSQL container already removed"
        
        echo "✅ Development environment stopped!"
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
        
        # Check CloudFlared container
        echo "CloudFlared Container:"
        if docker-compose ps cloudflared | grep -q "Up"; then
            echo "✅ CloudFlared container is running"
            echo "🌐 Application available at: https://htlyons.golfleaguemanager.app"
        else
            echo "❌ CloudFlared container is NOT running"
        fi

        # Check GitHub Runner container
        echo "GitHub Runner Container:"
        if docker-compose ps github-runner | grep -q "Up"; then
            echo "✅ GitHub Runner container is running"
            echo "🤖 Runner available for GitHub Actions workflows"
        else
            echo "❌ GitHub Runner container is NOT running"
            echo "ℹ️  Use './manage-services.sh runner-logs' to check runner status"
        fi
        
        echo ""
        
        # Check Native GitHub Runner
        echo "Native GitHub Runner:"
        RUNNER_DIR="$HOME/github-runner"
        if [ -d "$RUNNER_DIR" ]; then
            if pgrep -f "Runner.Listener" > /dev/null; then
                echo "✅ Native GitHub runner is running"
                echo "🤖 Runner available for GitHub Actions workflows"
            else
                echo "⚠️  Native GitHub runner is installed but not running"
                echo "ℹ️  Use './manage-services.sh runner-start' to start it"
            fi
        else
            echo "❌ Native GitHub runner is NOT installed"
            echo "ℹ️  Use './manage-services.sh native-runner' to install it"
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
        echo "=== CloudFlared Container Logs ==="
        echo ""
        docker-compose logs cloudflared --tail=30
        ;;
        
    runner-logs)
        echo "=== GitHub Runner Container Logs ==="
        echo ""
        docker-compose logs github-runner --tail=50
        ;;
        
    runner-token)
        echo "=== GitHub Runner Token Setup ==="
        echo ""
        echo "To set up the GitHub Runner token:"
        echo "1. Go to: https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners/new"
        echo "2. Copy the token provided"
        echo "3. Create .env file: cp .env.example .env"
        echo "4. Add your token: GITHUB_RUNNER_TOKEN=your_token_here"
        echo "5. Restart services: ./manage-services.sh restart"
        ;;
        
    cleanup-host)
        echo "=== Cleanup Host Services ==="
        echo ""
        echo "This will remove GitHub Runner and CloudFlared services from the host system."
        echo "Running cleanup script..."
        echo ""
        ./scripts/deployment/cleanup-host-services.sh
        ;;
        
    docker-build)
        echo "=== Building Docker Images ==="
        echo ""
        
        # Check if we're on Apple Silicon
        if [[ $(uname -m) == "arm64" ]]; then
            echo "ℹ️  Building on Apple Silicon (ARM64)..."
            echo "Using optimized ARM64 GitHub Actions runner binaries."
            echo ""
        fi
        
        echo "Building GitHub Runner container with ARM64 support..."
        docker-compose build github-runner --no-cache
        echo ""
        echo "Build complete!"
        ;;
        
    docker-rebuild)
        echo "=== Rebuilding GitHub Runner from Scratch ==="
        echo ""
        
        # Check if we're on Apple Silicon
        if [[ $(uname -m) == "arm64" ]]; then
            echo "ℹ️  Building on Apple Silicon (ARM64)..."
            echo "This will use native ARM64 binaries for optimal performance."
            echo ""
        fi
        
        echo "Stopping and removing existing container..."
        docker-compose down github-runner
        echo "Removing existing image..."
        docker rmi golfleaguemanager-github-runner:latest 2>/dev/null || echo "No existing image found"
        echo "Building new container..."
        docker-compose build github-runner --no-cache
        echo ""
        echo "Rebuild complete!"
        ;;
        
    docker-troubleshoot)
        echo "=== Docker Troubleshooting ==="
        echo ""
        echo "🔧 Attempting to resolve Docker I/O errors on ARM64 macOS..."
        echo ""
        
        echo "1. Stopping Docker Desktop..."
        osascript -e 'quit app "Docker Desktop"'
        sleep 5
        
        echo "2. Removing corrupted Docker data..."
        rm -rf ~/Library/Containers/com.docker.docker/Data/vms/0/data/Docker.raw 2>/dev/null || echo "No Docker.raw found"
        rm -rf ~/.docker/desktop-containerd/* 2>/dev/null || echo "No containerd data found"
        
        echo "3. Restarting Docker Desktop..."
        open -a "Docker Desktop"
        echo "   ⏳ Waiting for Docker to restart..."
        sleep 30
        
        echo "4. Waiting for Docker daemon to be ready..."
        local attempts=0
        while ! docker info > /dev/null 2>&1 && [ $attempts -lt 12 ]; do
            echo "   Waiting for Docker daemon... ($((attempts + 1))/12)"
            sleep 10
            attempts=$((attempts + 1))
        done
        
        if docker info > /dev/null 2>&1; then
            echo "✅ Docker is running"
            echo "5. Cleaning Docker system..."
            docker system prune -af --volumes
            echo "6. Pulling fresh base images..."
            docker pull --platform linux/arm64 ubuntu:22.04
        else
            echo "❌ Docker failed to start properly"
            echo ""
            echo "📋 Manual recovery steps:"
            echo "1. Open Docker Desktop Settings"
            echo "2. Go to 'Troubleshoot' tab"
            echo "3. Click 'Reset to factory defaults'"
            echo "4. Restart Docker Desktop"
            echo "5. Try building again"
        fi
        
        echo ""
        echo "7. Checking disk space..."
        df -h /
        
        echo "8. Checking Docker info..."
        docker info | grep -E "(Server Version|Storage Driver|Docker Root Dir)" 2>/dev/null || echo "Docker info not available"
        
        echo ""
        echo "💡 Alternative solutions for persistent I/O errors:"
        echo "   • Use './manage-services.sh docker-reset' for complete reset"
        echo "   • Use './manage-services.sh start-app' (skip GitHub runner container)"
        echo "   • Use './manage-services.sh build-minimal' for smaller builds"
        echo "   • Consider using native runner only (already working!)"
        ;;

    docker-reset)
        echo "=== Complete Docker Reset ==="
        echo ""
        echo "⚠️  This will completely reset Docker Desktop and remove all containers, images, and volumes."
        echo "Are you sure you want to continue? (y/N):"
        read -r confirm
        if [[ "$confirm" =~ ^[Yy]$ ]]; then
            echo ""
            echo "1. Stopping Docker Desktop..."
            osascript -e 'quit app "Docker Desktop"'
            sleep 5
            
            echo "2. Removing all Docker data..."
            rm -rf ~/Library/Containers/com.docker.docker/Data/vms
            rm -rf ~/.docker
            
            echo "3. Restarting Docker Desktop..."
            open -a "Docker Desktop"
            echo "   ⏳ Waiting for Docker to initialize..."
            sleep 60
            
            echo "4. Waiting for Docker daemon..."
            local attempts=0
            while ! docker info > /dev/null 2>&1 && [ $attempts -lt 20 ]; do
                echo "   Waiting... ($((attempts + 1))/20)"
                sleep 15
                attempts=$((attempts + 1))
            done
            
            if docker info > /dev/null 2>&1; then
                echo "✅ Docker reset complete and running"
                echo "5. Pulling essential base images..."
                docker pull --platform linux/arm64 ubuntu:22.04
                docker pull --platform linux/arm64 node:20
                docker pull --platform linux/arm64 mcr.microsoft.com/dotnet/aspnet:9.0
                docker pull --platform linux/arm64 nginx:alpine
            else
                echo "❌ Docker failed to start after reset"
            fi
        else
            echo "Reset cancelled."
        fi
        ;;

    build-minimal)
        echo "=== Building Minimal Docker Images ==="
        echo ""
        echo "Building only core application containers (no GitHub runner)..."
        
        # Build only backend and frontend
        echo "Building backend..."
        docker build --platform linux/arm64 -t golfleaguemanager-backend:latest ./backend || echo "Backend build failed"
        
        echo "Building frontend..."
        docker build --platform linux/arm64 -t golfleaguemanager-frontend:latest ./frontend || echo "Frontend build failed"
        
        echo ""
        echo "✅ Minimal build complete!"
        echo "Use './manage-services.sh start-app' to run without GitHub runner"
        ;;

    ci-build)
        echo "=== CI/CD Optimized Build ==="
        echo ""
        echo "Building with CI optimizations for ARM64 macOS..."
        
        # Set Docker BuildKit for better performance
        export DOCKER_BUILDKIT=1
        export COMPOSE_DOCKER_CLI_BUILD=1
        
        # Build with reduced parallelism to avoid I/O issues
        echo "Building with reduced resource usage..."
        docker-compose build --parallel --progress=plain
        
        if [ $? -eq 0 ]; then
            echo "✅ CI build successful!"
        else
            echo "❌ CI build failed. Trying fallback approach..."
            echo "Building containers sequentially..."
            
            # Try building one by one
            docker-compose build backend && \
            docker-compose build frontend && \
            docker-compose build github-runner
        fi
        ;;

    native-runner)
        echo "=== Setup Native GitHub Runner ==="
        echo ""
        echo "Setting up a native ARM64 GitHub Actions runner on your Mac."
        echo "This is often more reliable than Docker-based runners on macOS."
        echo ""
        ./scripts/deployment/setup-native-runner.sh
        ;;
        
    runner-start)
        echo "=== Starting Native GitHub Runner ==="
        echo ""
        RUNNER_DIR="$HOME/github-runner"
        if [ -d "$RUNNER_DIR" ]; then
            # Check if it's configured
            if [ ! -f "$RUNNER_DIR/.runner" ]; then
                echo "❌ Runner is not configured yet!"
                echo ""
                echo "Please configure it first:"
                echo "1. Go to: https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners/new"
                echo "2. Copy the registration token"
                echo "3. Run: cd $RUNNER_DIR && ./config.sh --url https://github.com/JuanMatute-Synergis/GolfLeagueManager --token YOUR_TOKEN"
                echo "4. Then start it with: ./manage-services.sh runner-start"
                exit 1
            fi
            
            # Check if already running
            if pgrep -f "Runner.Listener" > /dev/null; then
                echo "⚠️  Runner is already running"
                exit 0
            fi
            
            # Start the runner
            cd "$RUNNER_DIR"
            nohup ./run.sh > runner.log 2>&1 &
            sleep 2
            
            if pgrep -f "Runner.Listener" > /dev/null; then
                echo "✅ Native GitHub runner started successfully"
                echo "📍 Logs: $RUNNER_DIR/runner.log"
            else
                echo "❌ Failed to start runner. Check logs: $RUNNER_DIR/runner.log"
            fi
        else
            echo "❌ Native GitHub runner is NOT installed"
            echo "ℹ️  Use './manage-services.sh native-runner' to install it"
        fi
        ;;
        
    runner-stop)
        echo "=== Stopping Native GitHub Runner ==="
        echo ""
        if pgrep -f "Runner.Listener" > /dev/null; then
            # Stop the runner gracefully
            pkill -f "Runner.Listener"
            sleep 2
            
            if pgrep -f "Runner.Listener" > /dev/null; then
                echo "⚠️  Forcefully stopping runner..."
                pkill -9 -f "Runner.Listener"
            fi
            
            echo "✅ Native GitHub runner stopped"
        else
            echo "⚠️  Native GitHub runner is not running"
        fi
        ;;
        
    runner-config)
        echo "=== Configure Native GitHub Runner ==="
        echo ""
        RUNNER_DIR="$HOME/github-runner"
        if [ -d "$RUNNER_DIR" ]; then
            echo "📝 To configure your GitHub runner:"
            echo ""
            echo "1. Get a registration token from:"
            echo "   https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners/new"
            echo ""
            echo "2. Run the configuration command:"
            echo "   cd $RUNNER_DIR"
            echo "   ./config.sh --url https://github.com/JuanMatute-Synergis/GolfLeagueManager --token YOUR_TOKEN_HERE"
            echo ""
            echo "3. Start the runner:"
            echo "   ./manage-services.sh runner-start"
            echo ""
            echo "💡 Tip: The token is only valid for 1 hour, so configure it quickly!"
            echo ""
            echo "🚀 Would you like to configure it now? (y/N):"
            read -r configure_now
            if [[ "$configure_now" =~ ^[Yy]$ ]]; then
                echo ""
                echo "Please enter your GitHub registration token:"
                read -r github_token
                if [ -n "$github_token" ]; then
                    echo "Configuring runner..."
                    cd "$RUNNER_DIR"
                    ./config.sh --url https://github.com/JuanMatute-Synergis/GolfLeagueManager --token "$github_token"
                    echo ""
                    echo "✅ Configuration complete! Now you can start the runner with:"
                    echo "   ./manage-services.sh runner-start"
                else
                    echo "❌ No token provided. Please get a token and try again."
                fi
            fi
        else
            echo "❌ Native GitHub runner is NOT installed"
            echo "ℹ️  Use './manage-services.sh native-runner' to install it first"
        fi
        ;;

    runner-open)
        echo "=== Opening GitHub Runner Registration ==="
        echo ""
        GITHUB_URL="https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners/new"
        echo "Opening: $GITHUB_URL"
        if command -v open > /dev/null 2>&1; then
            open "$GITHUB_URL"
            echo "✅ Opened in your default browser"
        else
            echo "Please open this URL manually: $GITHUB_URL"
        fi
        echo ""
        echo "After getting the token, run:"
        echo "  ./manage-services.sh runner-config"
        ;;

    *)
        echo "Usage: $0 {start|start-app|dev-native|stop|stop-dev|restart|status|logs|runner-logs|runner-token|cleanup-host|docker-build|docker-rebuild|docker-troubleshoot|docker-reset|build-minimal|ci-build|native-runner|runner-start|runner-stop|runner-config|runner-open}"
        echo ""
        echo "Commands:"
        echo "  start        - Start all services (Docker + CloudFlared + GitHub Runner)"
        echo "  start-app    - Start application services only (no GitHub runner container)"
        echo "  dev-native   - Start development mode (native processes, minimal Docker)"
        echo "  stop         - Stop all services"
        echo "  stop-dev     - Stop development mode (native processes)"
        echo "  restart      - Restart all services"
        echo "  status       - Show status of all services"
        echo "  logs         - Show CloudFlared service logs"
        echo "  runner-logs  - Show GitHub Runner container logs"
        echo "  runner-token - Show instructions for setting up GitHub Runner token"
        echo "  cleanup-host - Remove old host-based services (GitHub Runner + CloudFlared)"
        echo "  docker-build - Build GitHub Runner Docker image"
        echo "  docker-rebuild - Rebuild GitHub Runner from scratch"
        echo "  docker-troubleshoot - Troubleshoot Docker I/O errors on ARM64 macOS"
        echo "  docker-reset - Complete Docker Desktop reset (removes all data)"
        echo "  build-minimal - Build only core app containers (backend + frontend)"
        echo "  ci-build     - CI/CD optimized build with fallbacks"
        echo "  native-runner - Setup native ARM64 GitHub Actions runner (recommended for macOS)"
        echo "  runner-start - Start the native GitHub runner"
        echo "  runner-stop  - Stop the native GitHub runner"
        echo "  runner-config - Configure the native GitHub runner (interactive)"
        echo "  runner-open  - Open GitHub runner registration page in browser"
        exit 1
        ;;
esac
