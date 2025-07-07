#!/bin/bash

# Setup GitHub Actions Runner in Container
# Golf League Manager - Containerized Runner Setup

echo "🤖 Setting up GitHub Actions Runner in Container..."
echo ""

# Check if .env file exists
if [[ ! -f ".env" ]]; then
    echo "📝 Creating .env file from template..."
    cp .env.example .env
    echo "✅ .env file created"
else
    echo "✅ .env file already exists"
fi

# Check if GITHUB_RUNNER_TOKEN is set
if grep -q "GITHUB_RUNNER_TOKEN=your_github_runner_token_here" .env 2>/dev/null || ! grep -q "GITHUB_RUNNER_TOKEN=" .env 2>/dev/null; then
    echo ""
    echo "⚠️  GitHub Runner Token Setup Required"
    echo ""
    echo "To complete the setup:"
    echo "1. Go to: https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners/new"
    echo "2. Select 'Linux' as the operating system"
    echo "3. Copy the registration token (starts with 'A...')"
    echo "4. Edit .env file and replace 'your_github_runner_token_here' with your token"
    echo ""
    echo "Example:"
    echo "GITHUB_RUNNER_TOKEN=A3V2C7N4Q8P..."
    echo ""
    echo "Then run: ./manage-services.sh restart"
    echo ""
    exit 0
fi

echo "✅ GitHub Runner token is configured"
echo ""

# Build the runner container
echo "🏗️ Building GitHub Runner container..."
docker-compose build github-runner
echo "✅ GitHub Runner container built"

echo ""

# Start all services
echo "🚀 Starting all services including GitHub Runner..."
docker-compose up -d
echo "✅ All services started"

echo ""

# Wait for services to initialize
echo "⏳ Waiting for services to initialize..."
sleep 15

echo ""

# Check status
echo "📋 Checking service status..."
./scripts/deployment/manage-services.sh status

echo ""
echo "🎉 Setup completed!"
echo ""
echo "Next steps:"
echo "• Check runner status: ./scripts/deployment/manage-services.sh status"
echo "• View runner logs: ./scripts/deployment/manage-services.sh runner-logs"
echo "• Test with a commit to trigger CI/CD workflow"
echo ""
echo "Your GitHub runner is now containerized and running alongside your other services!"
echo ""
