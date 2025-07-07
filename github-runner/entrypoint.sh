#!/bin/bash

set -e

# Environment variables with defaults
RUNNER_NAME=${RUNNER_NAME:-"dockerized-runner"}
RUNNER_WORKDIR=${RUNNER_WORKDIR:-"/tmp/github-runner-workdir"}
RUNNER_REPOSITORY_URL=${RUNNER_REPOSITORY_URL:-""}
RUNNER_TOKEN=${RUNNER_TOKEN:-""}

echo "🚀 Starting GitHub Actions Runner in Container"
echo "Runner Name: $RUNNER_NAME"
echo "Repository: $RUNNER_REPOSITORY_URL"
echo "Work Directory: $RUNNER_WORKDIR"

# Validate required environment variables
if [ -z "$RUNNER_TOKEN" ]; then
    echo "❌ ERROR: RUNNER_TOKEN environment variable is required"
    echo "Generate a token at: https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners/new"
    exit 1
fi

if [ -z "$RUNNER_REPOSITORY_URL" ]; then
    echo "❌ ERROR: RUNNER_REPOSITORY_URL environment variable is required"
    exit 1
fi

# Create work directory
mkdir -p "$RUNNER_WORKDIR"

# Configure the runner
echo "⚙️ Configuring GitHub Actions runner..."
./config.sh \
    --url "$RUNNER_REPOSITORY_URL" \
    --token "$RUNNER_TOKEN" \
    --name "$RUNNER_NAME" \
    --work "$RUNNER_WORKDIR" \
    --labels "docker,container,golf-league" \
    --unattended \
    --replace

# Function to handle shutdown
shutdown_runner() {
    echo "🛑 Shutting down runner..."
    ./config.sh remove --token "$RUNNER_TOKEN"
    exit 0
}

# Set up signal handlers
trap shutdown_runner SIGTERM SIGINT

# Start the runner
echo "✅ Starting GitHub Actions runner..."
exec ./run.sh
