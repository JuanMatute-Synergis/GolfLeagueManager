# Contributing to Golf League Manager

Thank you for your interest in contributing to Golf League Manager! This document provides guidelines and information for contributors.

## 🎯 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Contributing Guidelines](#contributing-guidelines)
- [Pull Request Process](#pull-request-process)
- [Code Standards](#code-standards)
- [Testing Requirements](#testing-requirements)
- [Documentation](#documentation)

## 📜 Code of Conduct

This project and everyone participating in it is governed by our Code of Conduct. By participating, you are expected to uphold this code.

### Our Standards

- Use welcoming and inclusive language
- Be respectful of differing viewpoints and experiences
- Gracefully accept constructive criticism
- Focus on what is best for the community
- Show empathy towards other community members

## 🚀 Getting Started

### Prerequisites

Before contributing, ensure you have:

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download)
- [Node.js 18+](https://nodejs.org/)
- [Angular CLI](https://angular.io/cli)
- [Git](https://git-scm.com/)
- IDE: [Visual Studio Code](https://code.visualstudio.com/) or [Visual Studio](https://visualstudio.microsoft.com/)

### First Time Setup

1. **Fork the repository**
   ```bash
   # Fork on GitHub, then clone your fork
   git clone https://github.com/your-username/GolfLeagueManager.git
   cd GolfLeagueManager
   ```

2. **Set up the upstream remote**
   ```bash
   git remote add upstream https://github.com/original-owner/GolfLeagueManager.git
   ```

3. **Install dependencies**
   ```bash
   # Backend
   cd backend
   dotnet restore
   
   # Frontend
   cd ../frontend
   npm install
   ```

4. **Set up environment**
   ```bash
   # Copy environment template
   cp .env.example .env
   # Edit .env with your local settings
   ```

5. **Run database migrations**
   ```bash
   cd backend
   dotnet ef database update
   ```

## 💻 Development Setup

### Running the Application

**Backend:**
```bash
cd backend
dotnet run
# API available at https://localhost:7230
```

**Frontend:**
```bash
cd frontend
npm start
# App available at http://localhost:4200
```

**Full Stack (Docker):**
```bash
docker-compose up -d
```

### Development Workflow

1. **Create a feature branch**
   ```bash
   git checkout -b feature/your-feature-name
   ```

2. **Make your changes**
   - Follow our [Code Standards](#code-standards)
   - Add tests for new functionality
   - Update documentation as needed

3. **Test your changes**
   ```bash
   # Backend tests
   cd backend && dotnet test
   
   # Frontend tests
   cd frontend && npm test
   
   # E2E tests
   cd frontend && npm run e2e
   ```

4. **Commit your changes**
   ```bash
   git add .
   git commit -m "feat: add new feature description"
   ```

## 🤝 Contributing Guidelines

### Types of Contributions

We welcome several types of contributions:

- **🐛 Bug fixes** - Fix existing issues
- **✨ New features** - Add new functionality
- **📚 Documentation** - Improve docs, README, comments
- **🎨 UI/UX improvements** - Enhance user interface
- **⚡ Performance** - Optimize code performance
- **🧪 Tests** - Add or improve test coverage
- **🔧 Tooling** - Improve development experience

### Issue Guidelines

Before starting work:

1. **Check existing issues** - Avoid duplicate work
2. **Create an issue** - Describe the problem or feature
3. **Get approval** - Wait for maintainer feedback on large changes
4. **Assign yourself** - Let others know you're working on it

### Branch Naming

Use descriptive branch names:

- `feature/player-handicap-tracking`
- `bugfix/scoring-calculation-error`
- `docs/api-documentation-update`
- `refactor/settings-component-cleanup`

## 📥 Pull Request Process

### Before Submitting

- [ ] Code follows our style guidelines
- [ ] Self-review of code changes
- [ ] Comments added for complex logic
- [ ] Tests added/updated for changes
- [ ] Documentation updated if needed
- [ ] No merge conflicts with main branch

### PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix (non-breaking change that fixes an issue)
- [ ] New feature (non-breaking change that adds functionality)
- [ ] Breaking change (fix or feature that causes existing functionality to not work as expected)
- [ ] Documentation update

## Testing
- [ ] Unit tests pass
- [ ] Integration tests pass
- [ ] Manual testing completed

## Screenshots (if applicable)
Add screenshots for UI changes

## Checklist
- [ ] My code follows the style guidelines
- [ ] I have performed a self-review
- [ ] I have commented my code where necessary
- [ ] I have made corresponding changes to documentation
- [ ] My changes generate no new warnings
- [ ] I have added tests that prove my fix is effective or that my feature works
- [ ] New and existing unit tests pass locally with my changes
```

### Review Process

1. **Automated checks** - CI/CD pipeline runs tests
2. **Code review** - Maintainers review changes
3. **Address feedback** - Make requested changes
4. **Approval** - Get approval from maintainers
5. **Merge** - Changes merged to main branch

## 📏 Code Standards

### .NET Core Backend

**Naming Conventions:**
- PascalCase for classes, methods, properties
- camelCase for local variables, parameters
- UPPER_CASE for constants

**Code Style:**
```csharp
// Good
public class PlayerService
{
    private readonly IPlayerRepository _playerRepository;
    
    public async Task<Player> GetPlayerAsync(Guid playerId)
    {
        return await _playerRepository.GetByIdAsync(playerId);
    }
}
```

**Architecture:**
- Follow SOLID principles
- Use dependency injection
- Implement repository pattern
- Use async/await for I/O operations

### Angular Frontend

**Naming Conventions:**
- kebab-case for file names
- PascalCase for classes
- camelCase for properties, methods

**Code Style:**
```typescript
// Good
export class PlayerSettingsComponent implements OnInit {
  players: Player[] = [];
  isLoading = false;
  
  constructor(private playerService: PlayerService) {}
  
  async loadPlayers(): Promise<void> {
    this.isLoading = true;
    try {
      this.players = await this.playerService.getPlayers().toPromise();
    } finally {
      this.isLoading = false;
    }
  }
}
```

**Component Structure:**
- Use standalone components where possible
- Implement reactive forms
- Handle loading and error states
- Follow Angular style guide

### Commit Messages

Use [Conventional Commits](https://conventionalcommits.org/):

```
<type>[optional scope]: <description>

[optional body]

[optional footer(s)]
```

**Types:**
- `feat`: new feature
- `fix`: bug fix
- `docs`: documentation only changes
- `style`: formatting, missing semicolons, etc.
- `refactor`: code change that neither fixes a bug nor adds a feature
- `test`: adding missing tests
- `chore`: maintain

**Examples:**
```
feat(players): add handicap tracking functionality
fix(scoring): correct point calculation for tied games
docs(readme): update installation instructions
refactor(settings): split monolithic component into modules
```

## 🧪 Testing Requirements

### Backend Testing

**Unit Tests:**
- Test business logic in services
- Mock dependencies using Moq
- Aim for >80% code coverage

```csharp
[Test]
public async Task GetPlayerAsync_WithValidId_ReturnsPlayer()
{
    // Arrange
    var playerId = Guid.NewGuid();
    var expectedPlayer = new Player { Id = playerId };
    _mockRepository.Setup(r => r.GetByIdAsync(playerId))
                   .ReturnsAsync(expectedPlayer);
    
    // Act
    var result = await _playerService.GetPlayerAsync(playerId);
    
    // Assert
    Assert.AreEqual(expectedPlayer, result);
}
```

**Integration Tests:**
- Test API endpoints
- Use TestServer
- Test database interactions

### Frontend Testing

**Unit Tests:**
- Test component logic
- Mock services
- Test user interactions

```typescript
describe('PlayersSettingsComponent', () => {
  it('should load players on init', () => {
    const mockPlayers = [{ id: 1, name: 'John Doe' }];
    playerService.getPlayers.and.returnValue(of(mockPlayers));
    
    component.ngOnInit();
    
    expect(component.players).toEqual(mockPlayers);
  });
});
```

**E2E Tests:**
- Test user workflows
- Use Playwright
- Test critical paths

## 📚 Documentation

### Code Documentation

**Backend:**
```csharp
/// <summary>
/// Retrieves a player by their unique identifier
/// </summary>
/// <param name="playerId">The unique identifier of the player</param>
/// <returns>The player if found, null otherwise</returns>
public async Task<Player?> GetPlayerAsync(Guid playerId)
```

**Frontend:**
```typescript
/**
 * Loads all players from the server and updates the component state
 * Handles loading states and error scenarios
 */
async loadPlayers(): Promise<void> {
```

### API Documentation

- Update Swagger documentation
- Provide request/response examples
- Document error codes and responses

### User Documentation

- Update README for new features
- Add screenshots for UI changes
- Update setup instructions if needed

## 🏷️ Release Process

### Versioning

We use [Semantic Versioning](https://semver.org/):

- **MAJOR** version for incompatible API changes
- **MINOR** version for backwards-compatible functionality
- **PATCH** version for backwards-compatible bug fixes

### Release Notes

Include in release notes:
- New features
- Bug fixes
- Breaking changes
- Migration instructions
- Known issues

## 🆘 Getting Help

### Communication Channels

- **GitHub Issues** - Bug reports, feature requests
- **GitHub Discussions** - General questions, ideas
- **Discord** - Real-time chat (if available)

### Resources

- [Project Wiki](https://github.com/your-repo/wiki)
- [API Documentation](https://localhost:7230/swagger)
- [Angular Style Guide](https://angular.io/guide/styleguide)
- [.NET Core Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)

## 🙏 Recognition

Contributors will be recognized in:
- README.md contributors section
- Release notes
- Annual contributor summary

Thank you for contributing to Golf League Manager! 🏌️‍♂️

---

*This contributing guide is inspired by best practices from the open source community.*
