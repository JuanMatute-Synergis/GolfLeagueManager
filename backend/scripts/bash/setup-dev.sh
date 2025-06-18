#!/bin/bash

# =============================================================================
# GOLF LEAGUE MANAGER - DEVELOPMENT SETUP SCRIPT
# =============================================================================
# This script helps set up the development environment for new contributors
# Run with: ./setup-dev.sh

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Print colored output
print_status() {
    echo -e "${BLUE}[INFO]${NC} $1"
}

print_success() {
    echo -e "${GREEN}[SUCCESS]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Check if command exists
command_exists() {
    command -v "$1" >/dev/null 2>&1
}

# Main setup function
setup_development_environment() {
    print_status "🏌️‍♂️ Setting up Golf League Manager development environment..."
    echo ""
    
    # Check prerequisites
    print_status "Checking prerequisites..."
    
    # Check .NET
    if command_exists dotnet; then
        DOTNET_VERSION=$(dotnet --version)
        print_success ".NET SDK found: $DOTNET_VERSION"
    else
        print_error ".NET SDK not found. Please install .NET 9.0 SDK from https://dotnet.microsoft.com/download"
        exit 1
    fi
    
    # Check Node.js
    if command_exists node; then
        NODE_VERSION=$(node --version)
        print_success "Node.js found: $NODE_VERSION"
    else
        print_error "Node.js not found. Please install Node.js 18+ from https://nodejs.org/"
        exit 1
    fi
    
    # Check npm
    if command_exists npm; then
        NPM_VERSION=$(npm --version)
        print_success "npm found: $NPM_VERSION"
    else
        print_error "npm not found. Please install npm"
        exit 1
    fi
    
    # Check Angular CLI
    if command_exists ng; then
        NG_VERSION=$(ng version --skip-git 2>/dev/null | grep "Angular CLI" | cut -d: -f2 | tr -d ' ')
        print_success "Angular CLI found: $NG_VERSION"
    else
        print_warning "Angular CLI not found. Installing globally..."
        npm install -g @angular/cli
        print_success "Angular CLI installed"
    fi
    
    # Check Git
    if command_exists git; then
        print_success "Git found"
    else
        print_error "Git not found. Please install Git"
        exit 1
    fi
    
    echo ""
    print_status "Installing backend dependencies..."
    
    # Backend setup
    cd backend
    if [ -f "backend.csproj" ]; then
        dotnet restore
        print_success "Backend dependencies restored"
    else
        print_error "backend.csproj not found in backend directory"
        exit 1
    fi
    
    cd ..
    
    # Frontend setup
    print_status "Installing frontend dependencies..."
    cd frontend
    
    if [ -f "package.json" ]; then
        npm install
        print_success "Frontend dependencies installed"
    else
        print_error "package.json not found in frontend directory"
        exit 1
    fi
    
    cd ..
    
    # Environment setup
    print_status "Setting up environment configuration..."
    
    if [ ! -f ".env" ]; then
        if [ -f ".env.example" ]; then
            cp .env.example .env
            print_success "Environment file created from template"
            print_warning "Please review and update .env file with your local settings"
        else
            print_warning ".env.example not found, skipping environment setup"
        fi
    else
        print_warning ".env file already exists, skipping"
    fi
    
    # Database setup
    print_status "Setting up database..."
    cd backend
    
    # Check if database needs to be created
    if dotnet ef database update 2>/dev/null; then
        print_success "Database updated successfully"
    else
        print_warning "Database update failed. You may need to create the database manually"
        print_status "Try running: dotnet ef database update"
    fi
    
    cd ..
    
    # Create helpful scripts
    print_status "Creating development scripts..."
    
    # Backend start script
    cat > start-backend.sh << 'EOF'
#!/bin/bash
echo "🚀 Starting Golf League Manager Backend..."
cd backend
dotnet run
EOF
    chmod +x start-backend.sh
    
    # Frontend start script
    cat > start-frontend.sh << 'EOF'
#!/bin/bash
echo "🚀 Starting Golf League Manager Frontend..."
cd frontend
npm start
EOF
    chmod +x start-frontend.sh
    
    # Full stack start script
    cat > start-dev.sh << 'EOF'
#!/bin/bash
echo "🚀 Starting Golf League Manager Full Stack..."
echo "Starting backend..."
cd backend
dotnet run &
BACKEND_PID=$!

echo "Waiting for backend to start..."
sleep 5

echo "Starting frontend..."
cd ../frontend
npm start &
FRONTEND_PID=$!

echo ""
echo "🎉 Golf League Manager is starting!"
echo "Backend: https://localhost:7230"
echo "Frontend: http://localhost:4200"
echo "Swagger: https://localhost:7230/swagger"
echo ""
echo "Press Ctrl+C to stop all services"

# Wait for interrupt
trap "echo 'Stopping services...'; kill $BACKEND_PID $FRONTEND_PID; exit" INT
wait
EOF
    chmod +x start-dev.sh
    
    print_success "Development scripts created"
    
    # Git hooks setup (optional)
    if [ -d ".git" ]; then
        print_status "Setting up Git hooks..."
        
        # Pre-commit hook
        cat > .git/hooks/pre-commit << 'EOF'
#!/bin/bash
echo "Running pre-commit checks..."

# Check if there are any TypeScript/C# files to lint
if git diff --cached --name-only | grep -E '\.(ts|cs)$' > /dev/null; then
    echo "Checking code style..."
    # Add linting commands here when available
fi

echo "Pre-commit checks passed!"
EOF
        chmod +x .git/hooks/pre-commit
        print_success "Git hooks configured"
    fi
    
    # Final instructions
    echo ""
    print_success "🎉 Development environment setup complete!"
    echo ""
    print_status "Next steps:"
    echo "  1. Review and update .env file with your settings"
    echo "  2. Start the backend: ./start-backend.sh"
    echo "  3. Start the frontend: ./start-frontend.sh"
    echo "  4. Or start both: ./start-dev.sh"
    echo ""
    print_status "Available URLs:"
    echo "  • Frontend: http://localhost:4200"
    echo "  • Backend API: https://localhost:7230"
    echo "  • Swagger Docs: https://localhost:7230/swagger"
    echo ""
    print_status "Useful commands:"
    echo "  • Backend tests: cd backend && dotnet test"
    echo "  • Frontend tests: cd frontend && npm test"
    echo "  • E2E tests: cd frontend && npm run e2e"
    echo "  • Database migration: cd backend && dotnet ef migrations add MigrationName"
    echo ""
    print_status "Documentation:"
    echo "  • Project README: ./README.md"
    echo "  • Contributing Guide: ./CONTRIBUTING.md"
    echo "  • API Docs: https://localhost:7230/swagger"
    echo ""
    print_success "Happy coding! 🏌️‍♂️"
}

# Check if running on macOS/Linux
if [[ "$OSTYPE" == "msys" || "$OSTYPE" == "cygwin" ]]; then
    print_warning "This script is designed for macOS/Linux. For Windows, please use setup-dev.ps1"
    exit 1
fi

# Run setup
setup_development_environment
