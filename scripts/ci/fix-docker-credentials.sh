#!/bin/bash

# Fix Docker credentials for CI environments (especially macOS self-hosted runners)
# This script removes keychain-based credential storage that causes issues in CI

set -e

echo "🔧 Fixing Docker credentials for CI environment..."

# Ensure Docker is running
if ! docker info >/dev/null 2>&1; then
    echo "❌ Docker daemon is not running. Please start Docker Desktop."
    exit 1
fi

# Create Docker config directory
mkdir -p ~/.docker

# Backup existing config if it exists
if [ -f ~/.docker/config.json ]; then
    echo "📦 Backing up existing Docker config..."
    cp ~/.docker/config.json ~/.docker/config.json.bak.$(date +%Y%m%d_%H%M%S)
fi

# Remove any existing credential store references completely
echo "📝 Creating clean Docker configuration..."
cat > ~/.docker/config.json << 'EOF'
{
  "auths": {},
  "experimental": "disabled"
}
EOF

# Remove any lingering credential store configurations
unset DOCKER_CREDENTIAL_STORE
unset DOCKER_CREDENTIAL_HELPER

# Also create a backup of PATH and modify it to exclude credential helpers
echo "🚫 Temporarily hiding credential helpers from PATH..."
export ORIGINAL_PATH="$PATH"

# Create a temporary bin directory with safe alternatives
mkdir -p ~/.docker/temp-bin

# Create dummy credential helpers that fail gracefully
cat > ~/.docker/temp-bin/docker-credential-osxkeychain << 'EOF'
#!/bin/bash
echo "Credential helper disabled for CI" >&2
exit 1
EOF

cat > ~/.docker/temp-bin/docker-credential-desktop << 'EOF'
#!/bin/bash
echo "Credential helper disabled for CI" >&2
exit 1
EOF

cat > ~/.docker/temp-bin/docker-credential-secretservice << 'EOF'
#!/bin/bash
echo "Credential helper disabled for CI" >&2
exit 1
EOF

chmod +x ~/.docker/temp-bin/docker-credential-*

# Prepend our temp bin to PATH to override system credential helpers
export PATH="$HOME/.docker/temp-bin:$PATH"
#!/bin/bash
echo "Credential helper disabled for CI" >&2
exit 1
EOF

chmod +x ~/.docker/temp-bin/docker-credential-*

# Prepend our temp bin to PATH to override system credential helpers
export PATH="$HOME/.docker/temp-bin:$PATH"

# Clear any cached credentials
echo "🧹 Clearing Docker credentials cache..."
docker logout >/dev/null 2>&1 || true

# Test Docker functionality
echo "🧪 Testing Docker functionality..."
if docker run --rm hello-world >/dev/null 2>&1; then
    echo "✅ Docker is working correctly"
else
    echo "⚠️  Docker test failed, but continuing..."
fi

# Set environment variables for current session
export DOCKER_CONFIG=~/.docker
export DOCKER_BUILDKIT=1
export COMPOSE_DOCKER_CLI_BUILD=1

echo "✅ Docker credentials fixed for CI environment"
echo "📋 Environment variables set:"
echo "   DOCKER_CONFIG=~/.docker"
echo "   DOCKER_BUILDKIT=1"
echo "   COMPOSE_DOCKER_CLI_BUILD=1"
echo "   PATH (credential helpers hidden)"

echo ""
echo "🔍 Current Docker configuration:"
cat ~/.docker/config.json

echo ""
echo "🐳 Docker version info:"
docker version --format 'Client: {{.Client.Version}}, Server: {{.Server.Version}}'

# Export the environment variables so they persist
echo "export DOCKER_CONFIG=~/.docker" >> ~/.bash_profile 2>/dev/null || true
echo "export DOCKER_BUILDKIT=1" >> ~/.bash_profile 2>/dev/null || true
echo "export COMPOSE_DOCKER_CLI_BUILD=1" >> ~/.bash_profile 2>/dev/null || true
