#!/bin/bash

# Setup GitHub Actions Self-Hosted Runner Natively on macOS ARM64
# This script sets up a native runner which will be faster than Docker on ARM64

RUNNER_DIR="$HOME/github-runner"
RUNNER_VERSION="2.325.0"

echo "=== Setting up GitHub Actions Self-Hosted Runner ==="
echo "This will create a native ARM64 runner on your Mac"
echo ""

# Check if runner directory already exists
if [ -d "$RUNNER_DIR" ]; then
    echo "⚠️  Runner directory already exists at $RUNNER_DIR"
    read -p "Do you want to remove it and start fresh? (y/N): " -n 1 -r
    echo ""
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        echo "Removing existing runner..."
        rm -rf "$RUNNER_DIR"
    else
        echo "Keeping existing installation. Exiting."
        exit 0
    fi
fi

# Create runner directory
echo "Creating runner directory..."
mkdir -p "$RUNNER_DIR"
cd "$RUNNER_DIR"

# Download GitHub Actions runner for ARM64 macOS
echo "Downloading GitHub Actions runner for ARM64 macOS..."
curl -o actions-runner-osx-arm64-${RUNNER_VERSION}.tar.gz -L https://github.com/actions/runner/releases/download/v${RUNNER_VERSION}/actions-runner-osx-arm64-${RUNNER_VERSION}.tar.gz

# Verify the download
if [ ! -f "actions-runner-osx-arm64-${RUNNER_VERSION}.tar.gz" ]; then
    echo "❌ Failed to download runner. Please check your internet connection."
    exit 1
fi

# Extract the installer
echo "Extracting runner..."
tar xzf ./actions-runner-osx-arm64-${RUNNER_VERSION}.tar.gz
rm actions-runner-osx-arm64-${RUNNER_VERSION}.tar.gz

# Install dependencies
echo "Installing dependencies..."
./bin/installdependencies.sh

echo ""
echo "✅ GitHub Actions runner has been downloaded and dependencies installed!"
echo ""
echo "🔧 Next steps:"
echo "1. Go to: https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners/new"
echo "2. Copy the registration token from GitHub"
echo "3. Run the configuration:"
echo "   cd $RUNNER_DIR"
echo "   ./config.sh --url https://github.com/JuanMatute-Synergis/GolfLeagueManager --token YOUR_TOKEN"
echo "4. Start the runner:"
echo "   ./run.sh"
echo ""
echo "💡 To run as a service, use: sudo ./svc.sh install && sudo ./svc.sh start"
echo ""
echo "📍 Runner location: $RUNNER_DIR"
