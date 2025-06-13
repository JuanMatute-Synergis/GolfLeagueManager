# 🏌️‍♂️ Golf League Manager

A comprehensive full-stack web application for managing golf leagues, player statistics, and tournament schedules.

![License](https://img.shields.io/badge/license-MIT-blue.svg)
![.NET Core](https://img.shields.io/badge/.NET%20Core-9.0-blue.svg)
![Angular](https://img.shields.io/badge/Angular-17-red.svg)
![TypeScript](https://img.shields.io/badge/TypeScript-5.0-blue.svg)

## 🎯 Overview

Golf League Manager is a modern web application designed to streamline the administration of golf leagues. It provides tools for player management, season scheduling, flight organization, scoring systems, and comprehensive statistics tracking.

### ✨ Key Features

- **👥 Player Management** - Complete CRUD operations for player profiles, handicaps, and contact information
- **🏆 Season & Flight Management** - Organize players into flights, manage multiple seasons
- **📊 Scoring System** - Configurable point systems for wins, losses, and ties
- **📅 Schedule Management** - Create and manage tournament schedules with templates
- **📈 Statistics & Reports** - Track player performance and league standings
- **🎨 Modern UI** - Responsive design with Bootstrap styling
- **🔧 Admin Dashboard** - Comprehensive settings and configuration management

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
├── docker-compose.yml         # Docker configuration
├── GolfLeagueManager.sln     # Visual Studio solution
└── README.md                 # This file
```

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
