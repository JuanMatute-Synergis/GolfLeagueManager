# GitHub Actions Workflows

This directory contains the CI/CD pipeline configuration for the Golf League Manager application.

## Workflows

### 1. `ci-cd.yml` - Main CI/CD Pipeline
**Triggers:** Push to `main` branch, Pull requests to `main`
- **Test Stage:** Runs tests for both backend (.NET) and frontend (Angular)
- **Build & Deploy Stage:** Builds Docker images and deploys containers
- **Database Migrations:** Runs migrations for all tenant databases
- **Verification:** Checks deployment health and service availability

### 2. `dev-build.yml` - Development Build
**Triggers:** Push to `develop` branch, feature branches, PRs to `develop`
- Lightweight build verification for development branches
- No deployment, just build and test validation

### 3. `hotfix.yml` - Hotfix Deployment
**Triggers:** Push to `hotfix/*` branches
- Quick deployment for urgent fixes
- Creates backup before deployment
- Minimal testing for faster deployment

### 4. `manual-deploy.yml` - Manual Deployment
**Triggers:** Manual trigger via GitHub Actions UI
- Allows manual deployment with options:
  - Choose environment (staging/production)
  - Skip tests (for emergencies)
  - Migration-only deployment
- Useful for controlled deployments and testing

### 5. `health-check.yml` - Scheduled Health Check
**Triggers:** Every 6 hours, or manual trigger
- Monitors system health
- Checks container resource usage
- Reviews logs for errors
- Reports disk usage

## Deployment Process

1. **Development:** Code pushed to feature branches triggers `dev-build.yml`
2. **Integration:** PRs to `main` trigger full test suite
3. **Production:** Merge to `main` triggers `ci-cd.yml` with full deployment
4. **Hotfixes:** Emergency fixes via `hotfix/*` branches
5. **Maintenance:** Manual deployments and scheduled health checks

## Prerequisites

- Self-hosted GitHub Actions runner installed and configured
- Docker and Docker Compose available on the runner
- PostgreSQL container accessible for testing
- Proper network configuration for health checks

## Scripts Used

- `migrate-all-tenants.sh` - Database migration script
- `verify-deployment.sh` - Deployment verification script
- `health-check.sh` - System health monitoring script

## Configuration

The workflows use the following environment variables:
- `POSTGRES_USER`: Database username
- `POSTGRES_PASSWORD`: Database password  
- `POSTGRES_DB`: Database name

## Monitoring

- All workflows provide detailed logs and status updates
- Failed deployments include container logs for debugging
- Health checks run automatically to catch issues early
- Manual triggers available for on-demand verification
