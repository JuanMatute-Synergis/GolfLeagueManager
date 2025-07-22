# Docker Credential Fix for CI/CD

## Problem
GitHub Actions CI/CD pipeline on macOS self-hosted runners was failing with Docker keychain errors:
```
error getting credentials - err: exit status 1, out: `keychain cannot be accessed because the current session does not allow user interaction. The keychain may be locked; unlock it by running "security -v unlock-keychain ~/Library/Keychains/login.keychain-db" and try again`
```

## Root Cause
- Docker on macOS uses credential helpers (`docker-credential-osxkeychain`, `docker-credential-desktop`) that interact with the macOS keychain
- In CI environments, these keychain interactions fail because there's no user session available for keychain access
- The Docker daemon tries to authenticate image pulls using these credential helpers, causing the build to fail

## Solution
We implemented a comprehensive Docker credential bypass system that:

1. **Completely replaces credential helpers** with dummy implementations during CI
2. **Creates a clean Docker configuration** without any credential store references
3. **Uses PATH manipulation** to override system credential helpers temporarily
4. **Validates the fix** by testing Docker operations before the main build

## Implementation

### New Scripts

#### `scripts/ci/docker-bypass-credentials.sh`
- Creates dummy credential helper scripts in `/tmp/docker-ci-bypass/`
- Prepends this directory to PATH to override system helpers
- Creates minimal Docker config without credential stores
- Tests Docker functionality before proceeding
- Sets up GitHub Actions environment variables

#### `scripts/ci/fix-docker-credentials.sh` (Enhanced)
- Backup approach with more comprehensive credential helper management
- Uses `~/.docker/temp-bin` directory for dummy helpers
- More detailed logging and validation

#### `scripts/ci/manage-credential-helpers.sh`
- Original script for moving/restoring credential helper binaries
- Less aggressive approach, kept for compatibility

### Workflow Changes

#### `.github/workflows/ci-cd.yml`
1. **Credential Bypass Step**: Runs `docker-bypass-credentials.sh` early in the workflow
2. **Verification Step**: Confirms credential helpers are properly bypassed
3. **Environment Variables**: Sets `DOCKER_CONFIG`, `DOCKER_BUILDKIT`, etc.
4. **Build Verification**: Adds debugging output before Docker builds
5. **Cleanup Step**: Removes temporary files and restores original state

## How It Works

1. **Setup Phase**:
   - Creates `/tmp/docker-ci-bypass/` with dummy credential helpers
   - Modifies PATH to prioritize dummy helpers over system ones
   - Creates clean `~/.docker/config.json` without credential stores

2. **Dummy Credential Helpers**:
   ```bash
   # When Docker calls docker-credential-osxkeychain get
   # Our dummy script returns empty credentials instead of failing
   {"ServerURL":"","Username":"","Secret":""}
   ```

3. **Docker Operations**:
   - Docker tries to use credential helpers for authentication
   - Gets dummy helpers that don't interact with keychain
   - Falls back to anonymous/public image pulls (which is fine for our use case)

4. **Cleanup Phase**:
   - Removes `/tmp/docker-ci-bypass/` directory
   - Restores original PATH and Docker configuration

## Testing

The solution was tested locally and validates:
- ✅ Dummy credential helpers respond correctly
- ✅ Docker can pull public images without keychain access
- ✅ Environment variables are properly set for GitHub Actions
- ✅ Cleanup restores original state

## Usage

The fix is automatically applied in the CI/CD workflow. For manual testing:

```bash
# Apply the bypass
./scripts/ci/docker-bypass-credentials.sh

# Test Docker operations
docker pull hello-world

# Clean up (if needed)
rm -rf /tmp/docker-ci-bypass
```

## Benefits

1. **No sudo required**: Works in restricted CI environments
2. **Temporary**: Only affects the current session
3. **Safe**: Doesn't modify system files permanently
4. **Comprehensive**: Handles all known credential helpers
5. **Validated**: Tests Docker functionality before proceeding

## Fallback Options

If this approach doesn't work:
1. The original `fix-docker-credentials.sh` script using `~/.docker/temp-bin`
2. The `manage-credential-helpers.sh` script for file-based management
3. Manual keychain unlocking (requires user interaction, not suitable for CI)

## Future Improvements

- Monitor for new credential helpers that might need dummy implementations
- Consider Docker registry authentication if private images are needed
- Optimize cleanup timing based on workflow requirements
