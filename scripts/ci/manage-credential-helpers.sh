#!/bin/bash

# Temporarily disable Docker credential helpers for CI
# This script moves credential helpers out of the way during CI runs

set -e

echo "🔧 Temporarily disabling Docker credential helpers for CI..."

# Function to disable a credential helper
disable_credential_helper() {
    local helper_path="$1"
    local helper_name="$2"
    
    if [ -f "$helper_path" ]; then
        local backup_path="${helper_path}.ci-disabled"
        echo "🚫 Disabling $helper_name..."
        mv "$helper_path" "$backup_path"
        echo "✅ $helper_name disabled (backed up to $backup_path)"
    fi
}

# Function to restore a credential helper
restore_credential_helper() {
    local helper_path="$1"
    local helper_name="$2"
    
    local backup_path="${helper_path}.ci-disabled"
    if [ -f "$backup_path" ]; then
        echo "🔄 Restoring $helper_name..."
        mv "$backup_path" "$helper_path"
        echo "✅ $helper_name restored"
    fi
}

# Main logic
case "${1:-disable}" in
    "disable")
        echo "🚫 Disabling credential helpers..."
        disable_credential_helper "/usr/local/bin/docker-credential-osxkeychain" "osxkeychain helper"
        disable_credential_helper "/usr/local/bin/docker-credential-desktop" "desktop helper"
        disable_credential_helper "/opt/homebrew/bin/docker-credential-osxkeychain" "osxkeychain helper (homebrew)"
        disable_credential_helper "/opt/homebrew/bin/docker-credential-desktop" "desktop helper (homebrew)"
        echo "✅ Credential helpers disabled for CI"
        ;;
    "restore")
        echo "🔄 Restoring credential helpers..."
        restore_credential_helper "/usr/local/bin/docker-credential-osxkeychain" "osxkeychain helper"
        restore_credential_helper "/usr/local/bin/docker-credential-desktop" "desktop helper"
        restore_credential_helper "/opt/homebrew/bin/docker-credential-osxkeychain" "osxkeychain helper (homebrew)"
        restore_credential_helper "/opt/homebrew/bin/docker-credential-desktop" "desktop helper (homebrew)"
        echo "✅ Credential helpers restored"
        ;;
    *)
        echo "Usage: $0 [disable|restore]"
        echo "  disable - Temporarily disable credential helpers for CI"
        echo "  restore - Restore credential helpers after CI"
        exit 1
        ;;
esac
