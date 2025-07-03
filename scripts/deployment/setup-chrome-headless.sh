#!/bin/bash

# Setup Chrome for headless testing on macOS GitHub Actions runner
echo "🌐 Setting up Chrome for headless testing..."

# Check if Chrome is already installed
if [ -f "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" ]; then
    echo "✅ Google Chrome found in Applications"
    
    # Create symlink to make Chrome available in PATH
    sudo ln -sf "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" /usr/local/bin/google-chrome 2>/dev/null || true
    sudo ln -sf "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" /usr/local/bin/chrome 2>/dev/null || true
    
    # Add to PATH for current session
    export PATH="/Applications/Google Chrome.app/Contents/MacOS:$PATH"
    
    # Test Chrome headless
    if "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" --version >/dev/null 2>&1; then
        echo "✅ Chrome headless test successful"
        "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome" --version
    else
        echo "⚠️  Chrome headless test failed"
    fi
else
    echo "❌ Google Chrome not found in /Applications"
    echo "Please install Google Chrome manually"
fi

echo "🔧 Chrome setup completed"
