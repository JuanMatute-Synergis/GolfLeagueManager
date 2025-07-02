#!/bin/bash

# Script to toggle authorization for debugging purposes
# Usage: ./toggle-auth.sh [on|off]

APPSETTINGS_FILE="/Users/juanmatute/Projects/GolfLeagueManager/backend/appsettings.Development.json"

if [ "$1" = "off" ]; then
    echo "🔓 Disabling authorization for debugging..."
    # Set DisableAuthorization to true
    sed -i '' 's/"DisableAuthorization": false/"DisableAuthorization": true/' "$APPSETTINGS_FILE"
    echo "✅ Authorization disabled. Restart the backend server to apply changes."
    echo "⚠️  WARNING: This should only be used in development for debugging purposes!"
elif [ "$1" = "on" ]; then
    echo "🔒 Enabling authorization..."
    # Set DisableAuthorization to false
    sed -i '' 's/"DisableAuthorization": true/"DisableAuthorization": false/' "$APPSETTINGS_FILE"
    echo "✅ Authorization enabled. Restart the backend server to apply changes."
else
    echo "Usage: $0 [on|off]"
    echo "  on  - Enable authorization (default/secure mode)"
    echo "  off - Disable authorization (debugging mode)"
    echo ""
    echo "Current setting:"
    grep -A 1 '"Debug"' "$APPSETTINGS_FILE" | grep "DisableAuthorization"
fi
