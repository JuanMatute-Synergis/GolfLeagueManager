# Debugging Authentication

For development and debugging purposes, you can easily disable authorization in the backend.

## Method 1: Using the Toggle Script (Recommended)

```bash
# Disable authorization for debugging
./toggle-auth.sh off

# Enable authorization (normal mode)
./toggle-auth.sh on

# Check current setting
./toggle-auth.sh
```

## Method 2: Manual Configuration

Edit `backend/appsettings.Development.json` and set:

```json
{
  "Debug": {
    "DisableAuthorization": true
  }
}
```

## Method 3: Use Debug Environment

Use the pre-configured debug settings:

```bash
cd backend
ASPNETCORE_ENVIRONMENT=Debug dotnet run
```

## Important Notes

⚠️ **WARNING**: Authorization bypass only works in Development environment for security reasons.

When authorization is disabled, you'll see warning messages:
- `⚠️ WARNING: Authorization is DISABLED for debugging purposes!`
- `⚠️ WARNING: Authentication and Authorization middleware DISABLED for debugging!`

Remember to **restart the backend server** after changing the configuration.

## Use Cases

This is useful for:
- Testing API endpoints without authentication
- Debugging database queries
- Quick data comparisons (like handicap analysis)
- Frontend development without login requirements
