#!/bin/bash

# Quick test script to validate CI/CD setup
echo "🧪 Testing CI/CD Setup Components"
echo "================================="

echo ""
echo "1. 🐳 Docker Status:"
if docker --version >/dev/null 2>&1; then
    echo "✅ Docker available"
    echo "   Version: $(docker --version)"
else
    echo "❌ Docker not available"
fi

echo ""
echo "2. 📦 Node.js and npm:"
if node --version >/dev/null 2>&1; then
    echo "✅ Node.js available"
    echo "   Version: $(node --version)"
else
    echo "❌ Node.js not available"
fi

if npm --version >/dev/null 2>&1; then
    echo "✅ npm available"
    echo "   Version: $(npm --version)"
else
    echo "❌ npm not available"
fi

echo ""
echo "3. 🔧 .NET SDK:"
if dotnet --version >/dev/null 2>&1; then
    echo "✅ .NET SDK available"
    echo "   Version: $(dotnet --version)"
else
    echo "❌ .NET SDK not available"
fi

echo ""
echo "4. 🌐 Chrome for testing:"
if command -v google-chrome >/dev/null 2>&1; then
    echo "✅ Chrome available in PATH"
    google-chrome --version 2>/dev/null || echo "   (Version check failed)"
elif [ -f "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" ]; then
    echo "⚠️  Chrome found in Applications but not in PATH"
    "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" --version 2>/dev/null || echo "   (Version check failed)"
else
    echo "❌ Chrome not found"
fi

echo ""
echo "5. 📁 Frontend dependencies:"
if [ -f "frontend/package-lock.json" ]; then
    echo "✅ package-lock.json exists"
else
    echo "❌ package-lock.json missing"
fi

if [ -f "frontend/postcss.config.js" ]; then
    echo "✅ postcss.config.js configured"
else
    echo "❌ postcss.config.js missing"
fi

echo ""
echo "6. 🏃‍♂️ GitHub Actions Runner:"
if ps aux | grep -q "Runner.Listener" | grep -v grep; then
    echo "✅ GitHub Actions runner process found"
else
    echo "⚠️  GitHub Actions runner process not detected"
fi

echo ""
echo "🎯 Setup validation complete!"
