# 🏌️‍♂️ Golf League Manager

A comprehensive full-stack web application for managing golf leagues, player statistics, match play scoring, and tournament administration with advanced features for absence handling and average score tracking.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET Core](https://img.shields.io/badge/.NET%20Core-9.0-blue.svg)
![Angular](https://img.shields.io/badge/Angular-17-red.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-blue.svg)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-blue.svg)

## 🎯 Overview

Golf League Manager is a modern, feature-rich web application designed to streamline the complete administration of golf leagues. It provides comprehensive tools for player management, season organization, advanced match play scoring, flight management, absence handling, and detailed statistics tracking with real-time calculations.

### ✨ Core Features

#### 👥 **Player Management**
- Complete CRUD operations for player profiles
- Handicap tracking (initial and current)
- **Average Score System** with decimal precision
- Contact information management
- Flight assignments with history tracking
- Player statistics and performance metrics

#### 🏆 **Season & Competition Management**
- Multi-season organization and management
- **Week-based scoring toggles** (enable/disable weeks from calculations)
- Flight organization with handicap-based assignments
- Flexible scoring system configuration
- Holiday week detection and management
- Season standings and leaderboards

#### ⛳ **Advanced Match Play Scoring**
- **Hole-by-hole match play scoring** with net score calculations
- **Triple scoring display**: Match Play, Gross, and Net scores
- Handicap stroke allocation by hole difficulty
- **2-point match bonus** for lowest net score
- Proper tie handling in match play scenarios
- Real-time point calculations and updates

#### 🚫 **Comprehensive Absence Handling**
- **Absent with Notice**: 4 points awarded to absent player
- **Absent without Notice**: 0 points awarded
- **Present Player Scoring**: 
  - 16 points if beating average score by a whole number
  - 8 points if not beating average significantly
- **Smart average comparison**: If average is 43.99, must shoot 42 or better for 16 points
- Dual absence scenarios with proper point distribution

#### 📊 **Advanced Scoring & Statistics**
- **Average Score Tracking** with decimal precision (e.g., 43.75, 39.99)
- Initial and current average score calculations
- Week-specific score entry and management
- Season standings with multiple sorting options
- Individual and team performance metrics
- Comprehensive leaderboards and reports

#### 📅 **Schedule & Week Management**
- **Interactive week management** with scoring toggles
- Visual indicators for week status (Active, Completed, Upcoming)
- Holiday week detection (Memorial Day, July 4th, Labor Day, Thanksgiving)
- Quick toggle for scoring/handicap inclusion
- Individual score count tracking
- Week reset and regeneration capabilities

#### 🎨 **Modern User Interface**
- **Responsive design** with Tailwind CSS
- Dark/light theme support
- Mobile-optimized layouts
- Interactive dashboards and real-time updates
- Intuitive navigation and user experience
- Progressive Web App (PWA) capabilities

#### 🔧 **Administrative Features**
- **Database cleanup tools** for data management
- **Import/export functionality** for player and matchup data
- Comprehensive settings management
- **Course management** with hole-by-hole configuration
- Flight assignment automation
- Data seeding and migration tools

## 🏗️ Architecture

### Backend (.NET Core 9.0)
- **API**: RESTful Web API with clean architecture
- **Database**: Entity Framework Core with SQL Server
- **Authentication**: JWT-based authentication (ready for implementation)
- **Logging**: Structured logging with Serilog
- **Documentation**: Swagger/OpenAPI integration

### Frontend (Angular 17)
- **Framework**: Angular with TypeScript
- **UI Library**: Bootstrap 5 with custom theming
- **State Management**: Reactive forms and services
- **Architecture**: Modular component-based design
- **Testing**: Karma + Jasmine unit tests
- **E2E Testing**: Playwright integration

## 🚀 Quick Start

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli): `npm install -g @angular/cli`
- SQL Server or SQLite (for development)

### 1. Clone the Repository

```bash
git clone https://github.com/yourusername/GolfLeagueManager.git
cd GolfLeagueManager
```

### 2. Backend Setup

```bash
# Navigate to backend directory
cd backend

# Restore NuGet packages
dotnet restore

# Update database
dotnet ef database update

# Run the backend API
dotnet run
```

The API will be available at `https://localhost:7230` (HTTPS) or `http://localhost:5289` (HTTP).

### 3. Frontend Setup

```bash
# Navigate to frontend directory
cd frontend

# Install npm packages
npm install

# Start the development server
npm start
```

The application will be available at `http://localhost:4200`.

### 4. Using Docker (Optional)

```bash
# Run the entire stack with Docker Compose
docker-compose up -d
```

## 📁 Project Structure

```
GolfLeagueManager/
├── backend/                    # .NET Core Web API
│   ├── Controllers/           # API controllers
│   ├── Models/               # Entity models & DTOs
│   ├── Business/             # Business logic services
│   ├── Data/                 # Data access repositories
│   ├── Migrations/           # EF Core migrations
│   └── Program.cs            # Application entry point
├── frontend/                  # Angular application
│   ├── src/app/
│   │   ├── modules/          # Feature modules
│   │   │   ├── settings/     # Settings management
│   │   │   │   ├── components/
│   │   │   │   │   ├── players-settings/
│   │   │   │   │   ├── seasons-settings/
│   │   │   │   │   ├── scoring-settings/
│   │   │   │   │   └── schedule-settings/
│   │   │   │   └── services/ # Settings services
│   │   │   ├── dashboard/    # Dashboard module
│   │   │   └── auth/         # Authentication module
│   │   ├── shared/           # Shared components
│   │   └── core/             # Core services
│   └── tests-e2e/            # End-to-end tests
├── frontend-old/              # Legacy frontend (deprecated)
├── scripts/                   # Utility scripts organized by category
│   ├── analysis/             # Data analysis and handicap scripts
│   ├── database/             # Database management scripts
│   ├── deployment/           # Deployment and infrastructure scripts
│   ├── testing/              # Testing and CI/CD scripts
│   ├── utilities/            # General utility scripts
│   └── README.md             # Scripts documentation
├── data/                      # Data files and analysis outputs
│   └── analysis/             # Analysis data and CSV files
├── docker-compose.yml         # Docker configuration
├── GolfLeagueManager.sln     # Visual Studio solution
└── README.md                 # This file
```

## 🔧 Scripts & Utilities

The project includes a comprehensive collection of utility scripts organized in the `/scripts` directory:

### Script Categories

- **`/scripts/analysis/`** - Data analysis, handicap calculations, and score pattern analysis
- **`/scripts/database/`** - Database management, tenant operations, and data updates
- **`/scripts/deployment/`** - Infrastructure, deployment, and service management scripts
- **`/scripts/testing/`** - Testing, CI/CD, and validation scripts
- **`/scripts/utilities/`** - General utility scripts and helpers

### Common Script Usage

```bash
# Database operations
./scripts/database/create-tenant.sh
./scripts/database/migrate-all-tenants.sh
./scripts/database/cleanup_duplicate_weeks.sh

# Testing and validation
./scripts/testing/test-multi-tenant.sh
./scripts/testing/test-ci-setup.sh

# Deployment and monitoring
./scripts/deployment/health-check.sh
./scripts/deployment/verify-deployment.sh

# Data analysis
./scripts/analysis/analyze_handicaps.py
./scripts/analysis/debug_week_analysis.py
```

📖 **See `/scripts/README.md` for detailed documentation of all available scripts and their usage.**

## 🔧 Configuration

### Backend Configuration

Update `appsettings.json` for your environment:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=GolfLeagueManager;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  }
}
```

### Frontend Configuration

Update `src/environments/environment.ts`:

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7230/api'
};
```

## 🧪 Testing

### Backend Tests
```bash
cd backend
dotnet test
```

### Frontend Tests
```bash
cd frontend

# Unit tests
npm test

# E2E tests
npm run e2e

# Coverage report
npm run test:coverage
```

## 📦 Deployment

### Production Build

**Backend:**
```bash
cd backend
dotnet publish -c Release -o ./publish
```

**Frontend:**
```bash
cd frontend
ng build --configuration production
```

### Docker Deployment

```bash
# Build and deploy with Docker
docker-compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

## 🔄 Development Workflow

### Adding New Features

1. **Backend**: Create controller, service, and model
2. **Frontend**: Generate component and service
3. **Testing**: Add unit and integration tests
4. **Documentation**: Update API documentation

### Database Changes

```bash
# Add new migration
dotnet ef migrations add MigrationName

# Update database
dotnet ef database update
```

## 📚 API Documentation

When running in development, visit:
- **Swagger UI**: `https://localhost:7230/swagger`
- **API Endpoints**: `https://localhost:7230/api`

### Key Endpoints

- `GET /api/players` - Get all players
- `POST /api/players` - Create new player
- `GET /api/seasons` - Get all seasons
- `GET /api/flights` - Get all flights
- `POST /api/flights` - Create new flight

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

### Code Standards

- **Backend**: Follow C# coding conventions and use StyleCop
- **Frontend**: Follow Angular style guide and use ESLint
- **Commits**: Use conventional commit messages
- **Testing**: Maintain >80% code coverage

## 🔧 Technology Stack

### Backend
- **.NET Core 9.0** - Web API framework
- **Entity Framework Core** - ORM for data access
- **SQL Server** - Primary database
- **AutoMapper** - Object mapping
- **FluentValidation** - Input validation
- **Swagger** - API documentation

### Frontend
- **Angular 17** - Frontend framework
- **TypeScript 5.0** - Programming language
- **Bootstrap 5** - UI framework
- **RxJS** - Reactive programming
- **Angular Material** - Component library (optional)
- **Chart.js** - Data visualization

### Development Tools
- **Visual Studio / VS Code** - IDEs
- **Docker** - Containerization
- **npm/yarn** - Package management
- **Git** - Version control
- **Playwright** - E2E testing

## 📈 Roadmap

### Phase 1 (Current)
- ✅ Basic player management
- ✅ Season and flight organization
- ✅ Settings management (refactored)
- ✅ Responsive UI

### Phase 2 (Planned)
- 🔄 Authentication & authorization
- 🔄 Real-time scoring
- 🔄 Advanced statistics
- 🔄 Mobile responsiveness

### Phase 3 (Future)
- 📱 Mobile app (PWA)
- 📊 Advanced analytics
- 🏆 Tournament management
- 📧 Email notifications
- 📱 SMS integration

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙋‍♂️ Support

- **Documentation**: [Wiki](https://github.com/yourusername/GolfLeagueManager/wiki)
- **Issues**: [GitHub Issues](https://github.com/yourusername/GolfLeagueManager/issues)
- **Discussions**: [GitHub Discussions](https://github.com/yourusername/GolfLeagueManager/discussions)

## 🎯 Recent Updates

### Version 2.0.0 (Latest)
- ✨ **Major Refactoring**: Settings module completely refactored into modular components
- 🏗️ **Architecture Improvement**: Split monolithic components into focused, maintainable modules
- 🎨 **UI Enhancement**: Modern Bootstrap styling with improved user experience
- 🧪 **Testing**: Comprehensive unit test coverage added
- ⚡ **Performance**: Optimized component loading and reduced bundle size
- 🔧 **Developer Experience**: Improved code organization and development workflow

---

**Made with ❤️ for golf enthusiasts and league administrators**

*Happy Golfing! 🏌️‍♂️⛳*
# Runner Test - Tue Jul  1 22:50:03 EDT 2025
