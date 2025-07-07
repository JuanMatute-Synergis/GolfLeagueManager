# Golf League Manager - CloudFlared Setup

## Application Access
Your Golf League Manager application is now accessible at:
**https://htlyons.golfleaguemanager.app**

## Service Management (Recommended)

The application runs entirely in Docker containers, including CloudFlared, and starts automatically with Docker Compose.

### Service Commands
```bash
# Check status of all services
./manage-services.sh status

# Start all services (including CloudFlared)
./manage-services.sh start

# Stop all services (including CloudFlared)
./manage-services.sh stop

# Restart all services
./manage-services.sh restart

# View CloudFlared logs
./manage-services.sh logs
```

## Manual Management (Alternative)

### Start Everything
```bash
# Start all Docker containers (including CloudFlared)
docker-compose up -d
```

### Stop Everything
```bash
# Stop all Docker containers (including CloudFlared)
docker-compose down
```

## Configuration Files
- `cloudflared-config.yml` - CloudFlared tunnel configuration
- `docker-compose.yml` - Docker services configuration

## Architecture
```
Internet → htlyons.golfleaguemanager.app → CloudFlared Container → frontend:80 (Frontend Container)
                                                                          ↓ /api/* requests
                                                                   backend:5505 (Backend Container)
                                                                          ↓
                                                                   postgres:5432 (Database Container)
```

## Container Details
- **CloudFlared**: Runs in its own container using `cloudflare/cloudflared:latest`
- **Tunnel ID**: 64746180-e8db-4616-abfb-0433703b2645
- **Tunnel Name**: golf-league-manager
- **Domain**: htlyons.golfleaguemanager.app
- **Frontend Container**: `frontend:80` (nginx)
- **Backend Container**: `backend:5505` (ASP.NET Core)
- **Database Container**: `postgres:5432` (PostgreSQL)

## Notes
- The nginx configuration in the frontend container handles routing `/api/*` requests to the backend
- All API calls from the web app are proxied through the frontend nginx to the backend container
- CloudFlared runs in its own container and connects to the frontend container via Docker networking
- All services are managed through Docker Compose for simplified deployment
- SSL/TLS is handled automatically by Cloudflare
- Container networking eliminates the need for localhost port binding conflicts
