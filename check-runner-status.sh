#!/bin/bash

# GitHub Actions Runner Status Checker

echo "🔍 GitHub Actions Self-Hosted Runner Status Check"
echo "================================================"
echo ""

# 1. Check for runner processes
echo "1. Checking for runner processes..."
runner_processes=$(ps aux | grep -E "(Runner\.Listener|Runner\.Worker|actions-runner)" | grep -v grep)
if [ -n "$runner_processes" ]; then
    echo "✅ Runner processes found:"
    echo "$runner_processes"
else
    echo "❌ No runner processes found"
    echo "   The GitHub Actions runner may not be installed or running"
fi
echo ""

# 2. Check for runner directories
echo "2. Checking for runner installation directories..."
possible_dirs=(
    "/Users/$(whoami)/actions-runner"
    "/opt/actions-runner"
    "/usr/local/actions-runner"
    "/Applications/actions-runner"
)

runner_found=false
for dir in "${possible_dirs[@]}"; do
    if [ -d "$dir" ]; then
        echo "✅ Runner directory found: $dir"
        runner_found=true
        
        # Check for configuration
        if [ -f "$dir/.runner" ]; then
            echo "   📁 Runner is configured"
            cat "$dir/.runner" 2>/dev/null | head -3
        else
            echo "   ⚠️  Runner not configured yet"
        fi
        
        # Check for service files
        if [ -f "$dir/svc.sh" ]; then
            echo "   📁 Service script found"
        fi
    fi
done

if [ "$runner_found" = false ]; then
    echo "❌ No runner installation directories found"
    echo "   You may need to install the GitHub Actions runner"
fi
echo ""

# 3. Check for runner services (macOS)
echo "3. Checking for runner services (macOS)..."
runner_services=$(launchctl list | grep -i "actions\|runner\|github" || echo "")
if [ -n "$runner_services" ]; then
    echo "✅ Runner services found:"
    echo "$runner_services"
else
    echo "❌ No runner services found in launchctl"
fi
echo ""

# 4. Instructions for checking in GitHub
echo "4. Manual verification in GitHub:"
echo "   🌐 Go to: https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners"
echo "   👀 Look for your runner with a green dot (online) or red dot (offline)"
echo ""

# 5. Test workflow trigger
echo "5. Test your runner:"
echo "   🚀 A test commit was just pushed to trigger the CI/CD pipeline"
echo "   📊 Check workflow status: https://github.com/JuanMatute-Synergis/GolfLeagueManager/actions"
echo ""

# 6. Next steps
echo "6. If runner is not working:"
echo "   📖 Installation guide: https://docs.github.com/en/actions/hosting-your-own-runners/adding-self-hosted-runners"
echo "   🔧 For macOS: Download runner from repository settings → Actions → Runners → New self-hosted runner"
echo "   ⚡ Run: ./config.sh --url https://github.com/JuanMatute-Synergis/GolfLeagueManager --token <TOKEN>"
echo "   🚀 Start: ./run.sh (or install as service: sudo ./svc.sh install && sudo ./svc.sh start)"
echo ""

echo "✨ Runner check completed!"
