# =============================================================================
# GOLF LEAGUE MANAGER - DEVELOPMENT SETUP SCRIPT (PowerShell)
# =============================================================================
# This script helps set up the development environment for Windows developers
# Run with: .\setup-dev.ps1

param(
    [switch]$SkipDependencyCheck,
    [switch]$Verbose
)

# Set error action
$ErrorActionPreference = "Stop"

# Colors for output
function Write-ColoredOutput {
    param(
        [string]$Message,
        [string]$Color = "White"
    )
    Write-Host $Message -ForegroundColor $Color
}

function Write-Status {
    param([string]$Message)
    Write-ColoredOutput "[INFO] $Message" -Color Cyan
}

function Write-Success {
    param([string]$Message)
    Write-ColoredOutput "[SUCCESS] $Message" -Color Green
}

function Write-Warning {
    param([string]$Message)
    Write-ColoredOutput "[WARNING] $Message" -Color Yellow
}

function Write-Error {
    param([string]$Message)
    Write-ColoredOutput "[ERROR] $Message" -Color Red
}

# Check if command exists
function Test-CommandExists {
    param([string]$Command)
    $null = Get-Command $Command -ErrorAction SilentlyContinue
    return $?
}

# Main setup function
function Setup-DevelopmentEnvironment {
    Write-Status "🏌️‍♂️ Setting up Golf League Manager development environment..."
    Write-Host ""
    
    if (-not $SkipDependencyCheck) {
        # Check prerequisites
        Write-Status "Checking prerequisites..."
        
        # Check .NET
        if (Test-CommandExists "dotnet") {
            $dotnetVersion = dotnet --version
            Write-Success ".NET SDK found: $dotnetVersion"
        }
        else {
            Write-Error ".NET SDK not found. Please install .NET 9.0 SDK from https://dotnet.microsoft.com/download"
            exit 1
        }
        
        # Check Node.js
        if (Test-CommandExists "node") {
            $nodeVersion = node --version
            Write-Success "Node.js found: $nodeVersion"
        }
        else {
            Write-Error "Node.js not found. Please install Node.js 18+ from https://nodejs.org/"
            exit 1
        }
        
        # Check npm
        if (Test-CommandExists "npm") {
            $npmVersion = npm --version
            Write-Success "npm found: $npmVersion"
        }
        else {
            Write-Error "npm not found. Please install npm"
            exit 1
        }
        
        # Check Angular CLI
        if (Test-CommandExists "ng") {
            Write-Success "Angular CLI found"
        }
        else {
            Write-Warning "Angular CLI not found. Installing globally..."
            npm install -g @angular/cli
            Write-Success "Angular CLI installed"
        }
        
        # Check Git
        if (Test-CommandExists "git") {
            Write-Success "Git found"
        }
        else {
            Write-Error "Git not found. Please install Git"
            exit 1
        }
    }
    
    Write-Host ""
    Write-Status "Installing backend dependencies..."
    
    # Backend setup
    Set-Location "backend"
    if (Test-Path "backend.csproj") {
        dotnet restore
        Write-Success "Backend dependencies restored"
    }
    else {
        Write-Error "backend.csproj not found in backend directory"
        exit 1
    }
    
    Set-Location ".."
    
    # Frontend setup
    Write-Status "Installing frontend dependencies..."
    Set-Location "frontend"
    
    if (Test-Path "package.json") {
        npm install
        Write-Success "Frontend dependencies installed"
    }
    else {
        Write-Error "package.json not found in frontend directory"
        exit 1
    }
    
    Set-Location ".."
    
    # Environment setup
    Write-Status "Setting up environment configuration..."
    
    if (-not (Test-Path ".env")) {
        if (Test-Path ".env.example") {
            Copy-Item ".env.example" ".env"
            Write-Success "Environment file created from template"
            Write-Warning "Please review and update .env file with your local settings"
        }
        else {
            Write-Warning ".env.example not found, skipping environment setup"
        }
    }
    else {
        Write-Warning ".env file already exists, skipping"
    }
    
    # Database setup
    Write-Status "Setting up database..."
    Set-Location "backend"
    
    try {
        dotnet ef database update
        Write-Success "Database updated successfully"
    }
    catch {
        Write-Warning "Database update failed. You may need to create the database manually"
        Write-Status "Try running: dotnet ef database update"
    }
    
    Set-Location ".."
    
    # Create helpful scripts
    Write-Status "Creating development scripts..."
    
    # Backend start script
    @"
@echo off
echo 🚀 Starting Golf League Manager Backend...
cd backend
dotnet run
pause
"@ | Out-File -FilePath "start-backend.bat" -Encoding ASCII
    
    # Frontend start script
    @"
@echo off
echo 🚀 Starting Golf League Manager Frontend...
cd frontend
npm start
pause
"@ | Out-File -FilePath "start-frontend.bat" -Encoding ASCII
    
    # PowerShell development script
    @'
# Golf League Manager Development Launcher
Write-Host "🚀 Starting Golf League Manager Full Stack..." -ForegroundColor Green

# Start backend
Write-Host "Starting backend..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd backend; dotnet run"

# Wait a moment for backend to start
Start-Sleep -Seconds 3

# Start frontend
Write-Host "Starting frontend..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd frontend; npm start"

Write-Host ""
Write-Host "🎉 Golf League Manager is starting!" -ForegroundColor Green
Write-Host "Backend: https://localhost:7230" -ForegroundColor Yellow
Write-Host "Frontend: http://localhost:4200" -ForegroundColor Yellow
Write-Host "Swagger: https://localhost:7230/swagger" -ForegroundColor Yellow
Write-Host ""
Write-Host "Press any key to exit..." -ForegroundColor Gray
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
'@ | Out-File -FilePath "start-dev.ps1" -Encoding UTF8
    
    Write-Success "Development scripts created"
    
    # Final instructions
    Write-Host ""
    Write-Success "🎉 Development environment setup complete!"
    Write-Host ""
    Write-Status "Next steps:"
    Write-Host "  1. Review and update .env file with your settings"
    Write-Host "  2. Start the backend: .\start-backend.bat"
    Write-Host "  3. Start the frontend: .\start-frontend.bat"
    Write-Host "  4. Or start both: .\start-dev.ps1"
    Write-Host ""
    Write-Status "Available URLs:"
    Write-Host "  • Frontend: http://localhost:4200"
    Write-Host "  • Backend API: https://localhost:7230"
    Write-Host "  • Swagger Docs: https://localhost:7230/swagger"
    Write-Host ""
    Write-Status "Useful commands:"
    Write-Host "  • Backend tests: cd backend; dotnet test"
    Write-Host "  • Frontend tests: cd frontend; npm test"
    Write-Host "  • E2E tests: cd frontend; npm run e2e"
    Write-Host "  • Database migration: cd backend; dotnet ef migrations add MigrationName"
    Write-Host ""
    Write-Status "Documentation:"
    Write-Host "  • Project README: .\README.md"
    Write-Host "  • Contributing Guide: .\CONTRIBUTING.md"
    Write-Host "  • API Docs: https://localhost:7230/swagger"
    Write-Host ""
    Write-Success "Happy coding! 🏌️‍♂️"
}

# Main execution
try {
    Setup-DevelopmentEnvironment
}
catch {
    Write-Error "Setup failed: $($_.Exception.Message)"
    exit 1
}
