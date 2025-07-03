#!/bin/bash

# Script to set up local development domains for multi-tenant testing
# This modifies /etc/hosts to map subdomains to localhost

echo "Setting up local development domains..."

# Backup current hosts file
sudo cp /etc/hosts /etc/hosts.backup.$(date +%Y%m%d_%H%M%S)

# Add local domain mappings
sudo tee -a /etc/hosts > /dev/null << 'EOF'

# Golf League Manager - Multi-Tenant Development
127.0.0.1    htlyons.golfleaguemanager.local
127.0.0.1    demo.golfleaguemanager.local
127.0.0.1    testclient.golfleaguemanager.local
127.0.0.1    dev.golfleaguemanager.local
EOF

echo "✅ Local domains configured!"
echo ""
echo "You can now access:"
echo "  • http://htlyons.golfleaguemanager.local:4500"
echo "  • http://demo.golfleaguemanager.local:4500" 
echo "  • http://testclient.golfleaguemanager.local:4500"
echo "  • http://dev.golfleaguemanager.local:4500"
echo ""
echo "To remove these domains later, restore from backup:"
echo "  sudo cp /etc/hosts.backup.* /etc/hosts"
