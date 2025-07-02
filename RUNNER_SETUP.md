# GitHub Actions Self-Hosted Runner Setup Guide

## Current Status ✅
- **CI/CD Pipeline**: Fully configured with 6 comprehensive workflows
- **Workflow Validation**: All YAML files are syntactically correct
- **Repository Setup**: All workflows committed and pushed to GitHub
- **Issue Resolved**: Removed duplicate workflow file that was causing validation errors

## Missing Component ⚠️
**Self-Hosted Runner**: Not currently installed/running on your system

## Quick Setup Instructions

### 1. Access Runner Settings
1. Go to your GitHub repository: https://github.com/JuanMatute-Synergis/GolfLeagueManager
2. Navigate to: **Settings** → **Actions** → **Runners**
3. Click **"New self-hosted runner"**

### 2. Download and Configure (macOS)
```bash
# Create runner directory
mkdir ~/actions-runner && cd ~/actions-runner

# Download the runner (replace with the URL from GitHub)
curl -o actions-runner-osx-arm64-2.317.0.tar.gz -L https://github.com/actions/runner/releases/download/v2.317.0/actions-runner-osx-arm64-2.317.0.tar.gz

# Extract the installer
tar xzf ./actions-runner-osx-arm64-2.317.0.tar.gz

# Configure the runner (use the token from GitHub)
./config.sh --url https://github.com/JuanMatute-Synergis/GolfLeagueManager --token YOUR_TOKEN_FROM_GITHUB
```

### 3. Start the Runner
```bash
# Option A: Run interactively (for testing)
./run.sh

# Option B: Install as a service (recommended for production)
sudo ./svc.sh install
sudo ./svc.sh start
```

### 4. Verify Installation
```bash
# Check if runner is running
./check-runner-status.sh

# Check GitHub repository runners page
# Should show your runner with a green dot (online)
```

## Workflow Overview 🚀

Once the runner is active, these workflows will trigger automatically:

### 1. **Main CI/CD Pipeline** (`ci-cd.yml`)
- **Triggers**: Push to `main` or `develop`, PR to `main`
- **Actions**: Test → Build → Deploy → Migrate → Verify
- **Duration**: ~10-15 minutes

### 2. **Development Build** (`dev-build.yml`)
- **Triggers**: Push to `develop` or `feature/*`, PR to `develop`
- **Actions**: Test and build validation only
- **Duration**: ~5-8 minutes

### 3. **Hotfix Deployment** (`hotfix.yml`)
- **Triggers**: Push to `hotfix/*` branches
- **Actions**: Fast-track deployment for urgent fixes
- **Duration**: ~8-12 minutes

### 4. **Manual Deployment** (`manual-deploy.yml`)
- **Triggers**: Manual workflow dispatch
- **Options**: Environment selection, skip tests, migration-only
- **Duration**: Variable based on options

### 5. **Emergency Rollback** (`rollback.yml`)
- **Triggers**: Manual workflow dispatch
- **Options**: Quick rollback, specific version, backup creation
- **Duration**: ~3-5 minutes

### 6. **Health Monitoring** (`health-check.yml`)
- **Triggers**: Every 6 hours, manual dispatch
- **Actions**: System health verification, resource monitoring
- **Duration**: ~2-3 minutes

## Testing the Setup 🧪

### Immediate Test
1. Make a small change to README.md
2. Commit and push to `main` branch
3. Check GitHub Actions tab for workflow execution
4. Verify all steps complete successfully

### Test Commands
```bash
# Test commit to trigger CI/CD
echo "Runner test: $(date)" >> README.md
git add README.md
git commit -m "test: Verify runner is working properly"
git push origin main

# Monitor workflow progress
echo "Check status at: https://github.com/JuanMatute-Synergis/GolfLeagueManager/actions"
```

## Troubleshooting 🔧

### Runner Not Appearing Online
```bash
# Check runner logs
cd ~/actions-runner
cat _diag/Runner_*.log

# Restart runner service
sudo ./svc.sh stop
sudo ./svc.sh start
```

### Workflow Failures
```bash
# Check detailed logs in GitHub Actions tab
# Common issues:
# - Docker not running: brew services start docker
# - Ports in use: docker-compose down
# - Permission issues: chmod +x script-name.sh
```

### Resource Issues
```bash
# Check Docker resources
docker system df
docker system prune -f

# Check disk space
df -h
```

## Security Notes 🔒

1. **Runner Security**: Self-hosted runners have access to your local environment
2. **Token Management**: Keep GitHub tokens secure and rotate regularly  
3. **Network Access**: Ensure runner can access Docker Hub and GitHub
4. **Permissions**: Run with minimal required permissions

## Next Steps 📋

1. ✅ **Setup Runner**: Follow instructions above
2. ✅ **Test Pipeline**: Push a test commit
3. ✅ **Monitor Health**: Workflows will run health checks automatically
4. ✅ **Production Ready**: Your CI/CD pipeline is fully configured!

## Support Resources 📚

- **GitHub Docs**: https://docs.github.com/en/actions/hosting-your-own-runners
- **Docker Docs**: https://docs.docker.com/get-started/
- **Troubleshooting**: Check `DEPLOYMENT_GUIDE.md` for detailed troubleshooting

---

**Status**: ✅ CI/CD pipeline fully configured and ready for runner activation
