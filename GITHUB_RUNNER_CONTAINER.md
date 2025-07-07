# GitHub Actions Runner - Container Setup

## Overview

Your GitHub Actions runner now runs in a Docker container alongside your other services for better isolation, consistency, and management.

## Quick Setup

### 1. Run the Setup Script
```bash
./scripts/deployment/setup-container-runner.sh
```

### 2. Get GitHub Token
1. Go to: https://github.com/JuanMatute-Synergis/GolfLeagueManager/settings/actions/runners/new
2. Select **Linux** as the operating system  
3. Copy the registration token (starts with 'A...')

### 3. Configure Token
```bash
# Edit .env file
nano .env

# Add your token:
GITHUB_RUNNER_TOKEN=A3V2C7N4Q8P...
```

### 4. Start Services
```bash
./scripts/deployment/manage-services.sh restart
```

## Container Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                    Docker Compose Services                 │
├─────────────────────────────────────────────────────────────┤
│  postgres          │  Database container                    │
│  backend           │  ASP.NET Core API container           │
│  frontend          │  Angular + nginx container            │
│  cloudflared       │  Cloudflare tunnel container          │
│  github-runner     │  GitHub Actions runner container      │
└─────────────────────────────────────────────────────────────┘
```

## Service Management

### Start All Services
```bash
./scripts/deployment/manage-services.sh start
```

### Check Status
```bash
./scripts/deployment/manage-services.sh status
```

### View Runner Logs
```bash
./scripts/deployment/manage-services.sh runner-logs
```

### Token Setup Help
```bash
./scripts/deployment/manage-services.sh runner-token
```

## Benefits of Container Approach

✅ **Unified Management** - All services controlled via Docker Compose  
✅ **Isolation** - Runner isolated from host system  
✅ **Consistency** - Same environment across deployments  
✅ **Dependencies** - All tools (Docker, .NET, Node.js) included  
✅ **Security** - Controlled access via container networking  
✅ **Scaling** - Easy to add more runners  

## Runner Features

- **OS**: Ubuntu 22.04 LTS
- **Tools**: Docker, Docker Compose, .NET 9.0, Node.js 22, Git
- **Labels**: `docker`, `container`, `golf-league`
- **Access**: Docker socket mounted for CI/CD builds
- **Work Directory**: `/tmp/github-runner-workdir` (mounted volume)

## Troubleshooting

### Runner Not Connecting
```bash
# Check logs
./scripts/deployment/manage-services.sh runner-logs

# Common issues:
# - Invalid token: Get new token from GitHub
# - Network issues: Check internet connectivity
# - Container build issues: Rebuild with docker-compose build github-runner
```

### Token Expired
```bash
# Get new token from GitHub
# Update .env file
# Restart services
./scripts/deployment/manage-services.sh restart
```

### Container Build Issues
```bash
# Rebuild runner container
docker-compose build github-runner --no-cache

# Check build logs
docker-compose build github-runner
```

## Migration from Host Runner

If you had a host-based runner:

1. **Stop old runner**:
   ```bash
   cd ~/actions-runner
   sudo ./svc.sh stop
   sudo ./svc.sh uninstall
   ```

2. **Remove old runner** from GitHub:
   - Go to repository Settings → Actions → Runners
   - Remove the old runner

3. **Setup container runner**:
   ```bash
   ./scripts/deployment/setup-container-runner.sh
   ```

## Security Notes

- Runner has access to Docker socket (required for CI/CD)
- Container runs with necessary privileges for builds
- All volumes are explicitly mounted (no full filesystem access)
- Network isolation via Docker Compose networking

## Support

- Check logs: `./scripts/deployment/manage-services.sh runner-logs`
- GitHub Docs: https://docs.github.com/en/actions/hosting-your-own-runners
- Docker Docs: https://docs.docker.com/

---

**Status**: ✅ GitHub Actions runner fully containerized and ready for CI/CD workflows
