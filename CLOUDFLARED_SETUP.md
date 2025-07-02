# Golf League Manager - CloudFlared Setup

## Application Access
Your Golf League Manager application is now accessible at:
**https://htlyons.golfleaguemanager.app**

## Service Management (Recommended)

The application is now set up as a system service that starts automatically on boot.

### Service Commands
```bash
# Check status of all services
./manage-services.sh status

# Start all services
./manage-services.sh start

# Stop all services
./manage-services.sh stop

# Restart all services
./manage-services.sh restart

# View CloudFlared logs
./manage-services.sh logs
```

## Manual Management (Alternative)

### Start Everything
```bash
# Start Docker containers
docker-compose up -d

# Start CloudFlared tunnel (run in background)
./start-tunnel.sh
```

### Stop Everything
```bash
# Stop CloudFlared tunnel
./stop-tunnel.sh

# Stop Docker containers
docker-compose down
```

## Configuration Files
- `cloudflared-config.yml` - CloudFlared tunnel configuration
- `docker-compose.yml` - Docker services configuration

## Architecture
```
Internet → htlyons.golfleaguemanager.app → CloudFlared Tunnel → localhost:4500 (Frontend)
                                                                     ↓ /api/* requests
                                                              localhost:5505 (Backend)
                                                                     ↓
                                                              localhost:5432 (Database)
```

## Tunnel Details
- **Tunnel ID**: 64746180-e8db-4616-abfb-0433703b2645
- **Tunnel Name**: golf-league-manager
- **Domain**: htlyons.golfleaguemanager.app
- **Local Frontend**: http://localhost:4500
- **Local Backend**: http://localhost:5505

## Notes
- The nginx configuration in the frontend container handles routing `/api/*` requests to the backend
- All API calls from the web app are proxied through the frontend nginx to the backend
- The CloudFlared tunnel only exposes the frontend port (4500) to the internet
- SSL/TLS is handled automatically by Cloudflare
