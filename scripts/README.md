# Scripts Directory

This directory contains all utility scripts for the Golf League Manager project, organized by category.

## Directory Structure

### `/analysis`
Scripts for data analysis and handicap calculations:
- `analyze_handicaps.py` - Handicap analysis and calculations
- `analyze_score_patterns.py` - Score pattern analysis
- `analyze_with_node.js` - Node.js-based analysis scripts
- `debug_week_analysis.py` - Week-specific debugging analysis
- `detailed_week_analysis.py` - Detailed week analysis
- `simple_analysis.py` - Simple data analysis scripts
- `simple_week_check.py` - Basic week validation
- `test_data.py` - Test data generation and validation

### `/database`
Database management and data manipulation scripts:
- `add_week_24.sh` - Add week 24 to database
- `cleanup_duplicate_weeks.sh` - Remove duplicate week entries
- `create-admin-user.py` - Create administrative users
- `create-tenant.sh` - Create new tenant
- `delete-tenant.sh` - Delete existing tenant
- `list-tenants.sh` - List all tenants
- `migrate-all-tenants.sh` - Run migrations for all tenants
- `regenerate_weeks.sh` - Regenerate week data
- `restore_matchup_weekids.sh` - Restore matchup week IDs
- `update_course_data_dynamic.sh` - Update course data dynamically
- `update_session1_averages.sh` - Update session 1 averages
- `update_session2_averages*.sh` - Various session 2 average updates

### `/deployment`
Deployment, infrastructure, and service management scripts:
- `check-runner-status*.sh` - Check GitHub runner status
- `fix-docker-credentials.sh` - Fix Docker credential issues
- `health-check.sh` - Application health checks
- `manage-services.sh` - Service management
- `rollback-deployment.sh` - Rollback deployments
- `setup-chrome-headless.sh` - Setup headless Chrome
- `setup-dev-domains.sh` - Setup development domains
- `setup-dev.ps1` - Development environment setup (PowerShell)
- `start-tunnel.sh` / `stop-tunnel.sh` - Manage tunneling
- `status.sh` - Check application status
- `verify-deployment.sh` - Verify deployment success

### `/testing`
Testing and CI/CD scripts:
- `test-ci-setup.sh` - CI/CD setup testing
- `test-multi-tenant-comprehensive.sh` - Comprehensive multi-tenant testing
- `test-multi-tenant.sh` - Basic multi-tenant testing

### `/utilities`
General utility scripts:
- `toggle-auth.sh` - Toggle authentication on/off

## Usage

All scripts should be run from the project root directory unless otherwise specified in the individual script documentation.

Example:
```bash
# From project root
./scripts/database/create-tenant.sh
./scripts/testing/test-multi-tenant.sh
./scripts/deployment/health-check.sh
```

## Notes

- Ensure scripts have proper execute permissions: `chmod +x script-name.sh`
- Some scripts may require environment variables or configuration files
- Check individual script headers for specific usage instructions
