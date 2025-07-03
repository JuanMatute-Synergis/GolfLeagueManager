#!/bin/bash

echo "🔧 Fixing Docker credentials for GitHub Actions..."

# Create Docker config directory if it doesn't exist
mkdir -p ~/.docker

# Configure Docker to not use credential helpers
cat > ~/.docker/config.json << 'EOF'
{
  "auths": {},
  "credsStore": ""
}
EOF

echo "✅ Docker credentials configuration updated"
echo "📝 Docker will now use direct authentication instead of keychain"

# Test Docker access
echo "🧪 Testing Docker access..."
if docker info > /dev/null 2>&1; then
    echo "✅ Docker is accessible"
else
    echo "❌ Docker access test failed"
    exit 1
fi

# Test image pull
echo "🐳 Testing image pull..."
if docker pull hello-world > /dev/null 2>&1; then
    echo "✅ Docker image pull successful"
    docker rmi hello-world > /dev/null 2>&1
else
    echo "❌ Docker image pull failed"
    exit 1
fi

echo "🎉 Docker credentials fix completed successfully!"
