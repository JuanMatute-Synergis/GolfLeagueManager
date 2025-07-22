#!/bin/bash

# Completely bypass Docker credential helpers for CI environments
# This is a more aggressive approach than the standard fix

set -e

echo "🚫 Completely bypassing Docker credential helpers..."

# Create a clean Docker config directory
mkdir -p ~/.docker

# Create the most minimal Docker config possible
cat > ~/.docker/config.json << 'EOF'
{
  "auths": {}
}
EOF

# Create temporary directory for dummy credential helpers
mkdir -p /tmp/docker-ci-bypass

# Create dummy credential helpers that do nothing
for helper in osxkeychain desktop secretservice wincred; do
    cat > "/tmp/docker-ci-bypass/docker-credential-$helper" << 'EOF'
#!/bin/bash
# Dummy credential helper for CI - does nothing
case "$1" in
    "get")
        echo '{"ServerURL":"","Username":"","Secret":""}'
        ;;
    "store")
        # Do nothing - just consume input
        cat >/dev/null
        ;;
    "erase")
        # Do nothing
        ;;
    *)
        exit 1
        ;;
esac
exit 0
EOF
    chmod +x "/tmp/docker-ci-bypass/docker-credential-$helper"
done

# Add our bypass directory to PATH
export PATH="/tmp/docker-ci-bypass:$PATH"

# Clear any existing Docker login sessions
docker logout 2>/dev/null || true

# Unset any credential-related environment variables
unset DOCKER_CREDENTIAL_STORE 2>/dev/null || true
unset DOCKER_CREDENTIAL_HELPER 2>/dev/null || true

echo "✅ Docker credential helpers completely bypassed"
echo "📋 Bypass directory: /tmp/docker-ci-bypass"
echo "🔍 Available credential helpers:"
ls -la /tmp/docker-ci-bypass/

# Test that Docker can pull images without credential issues
echo "🧪 Testing Docker image pull without credentials..."
if docker pull hello-world:latest >/dev/null 2>&1; then
    echo "✅ Docker pull test successful"
    docker rmi hello-world:latest >/dev/null 2>&1 || true
else
    echo "❌ Docker pull test failed"
    exit 1
fi

# Export PATH for subsequent steps (only in GitHub Actions)
if [ -n "$GITHUB_ENV" ]; then
    echo "PATH=/tmp/docker-ci-bypass:$PATH" >> $GITHUB_ENV
fi
