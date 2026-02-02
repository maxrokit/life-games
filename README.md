# Conway's Game of Life - Full Stack Application

[![.NET 8](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![React](https://img.shields.io/badge/React-18-61DAFB?logo=react)](https://react.dev/)
[![TypeScript](https://img.shields.io/badge/TypeScript-5-3178C6?logo=typescript)](https://www.typescriptlang.org/)
[![Tailwind CSS](https://img.shields.io/badge/Tailwind-v4-06B6D4?logo=tailwindcss)](https://tailwindcss.com/)
[![Tests](https://img.shields.io/badge/tests-39%20passing-success)](src/life-games-api)

> Production-ready implementation of Conway's Game of Life with Clean Architecture .NET 8 backend API and modern React frontend for testing API.

## ✨ Features

### 🎮 Interactive Game of Life
- **Development environment API test front end**:
- **Visual Grid**: Click to toggle cells, watch patterns evolve in real-time
- **Preset Patterns**: 7 classic patterns (Glider, Gosper Glider Gun, Blinker, Toad, Beacon, Pulsar, Pentadecathlon)
- **Dual Modes**:
  - **Local Mode**: Pure client-side computation (instant, no API calls)
  - **API Mode**: Server-side computation with persistent storage
- **Centered Grid**: Coordinate system centered at (0,0) with axis labels
- **Configurable Size**: Adjustable board dimensions (20x20 to 200x200, default 100x100)
- **Speed Control**: 1ms to 1000ms per generation (up to 1000 generations/second)
- **Final State Detection**: Find stable states, oscillators, and detect cycles

### 🏗️ Backend Architecture
- **Clean Architecture**: Domain → Application → Infrastructure → API
- **CQRS Pattern**: Commands and queries separated with MediatR
- **Aggregate Pattern**: Board as aggregate root managing Generations
- **Repository Pattern**: Single repository for entire aggregate
- **Navigation Properties**: 50% fewer database queries through EF Core optimization
- **Cycle Detection**: Identifies still lifes, oscillators, and patterns with long cycles
- **Rate Limiting**: 1000 requests/second per IP
- **HATEOAS Links**: Hypermedia controls for API discoverability
- **API Versioning**: Media type versioning (Accept: application/vnd.lifegames.v1+json)
- **JSON API**: RESTful JSON responses

### 🔒 Production Ready
- **Front End**: Front end is for testing API during development.  It is not intended for production release.
- **Structured Logging**: Serilog with correlation IDs
- **Health Checks**: Database connectivity monitoring
- **Input Validation**: FluentValidation with detailed error messages
- **Security Headers**: HSTS, X-Content-Type-Options, X-Frame-Options, CSP
- **Error Handling**: RFC 7807 Problem Details for all errors
- **Docker Support**: Multi-stage builds with health checks
- **Rate Limiting**: Configurable per-IP rate limits
- **Response Caching**: Optimized for read-heavy workloads

## 📋 Table of Contents

- [Quick Start](#-quick-start)
- [Project Structure](#-project-structure)
- [API Documentation](#-api-documentation)
- [Architecture Details](#-architecture-details)
- [Testing](#-testing)
- [Configuration](#-configuration)
- [Development](#-development)
- [Docker Deployment](#-docker-deployment)
- [Performance](#-performance)
- [Troubleshooting](#-troubleshooting)
- [Contributing](#-contributing)

## 🚀 Quick Start

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/) and npm
- No database installation required (SQLite file-based database)

### Backend API

```bash
cd src/life-games-api
dotnet restore
dotnet build
dotnet run --project Api/LifeGames.Api
```

✅ API running at: http://localhost:5253
✅ Swagger UI at: http://localhost:5253/swagger

### Frontend App

```bash
cd src/life-games-app
npm install
npm run dev
```

✅ App running at: http://localhost:5173

### Docker (Backend Only)

```bash
cd src/life-games-api
docker-compose up --build
```

✅ API running at: http://localhost:5253

## 📁 Project Structure

```
life-games/
├── src/
│   ├── life-games-api/              # .NET 8 Backend
│   │   ├── Domain/                  # Core business logic
│   │   │   ├── Entities/           # Board, BoardGeneration
│   │   │   ├── Services/           # GameOfLifeEngine
│   │   │   ├── ValueObjects/       # Cell
│   │   │   └── Interfaces/         # IBoardRepository
│   │   │
│   │   ├── Application/             # Use cases (CQRS)
│   │   │   ├── Handlers/           # MediatR command/query handlers
│   │   │   ├── Services/           # CycleDetectionService
│   │   │   ├── DTOs/               # Response DTOs with HATEOAS
│   │   │   └── Validators/         # FluentValidation rules
│   │   │
│   │   ├── Infrastructure/          # Data access
│   │   │   ├── Data/               # DbContext
│   │   │   ├── Repositories/       # BoardRepository
│   │   │   ├── Configurations/     # EF Core entity configs
│   │   │   └── Migrations/         # Database migrations
│   │   │
│   │   └── Api/                     # REST API
│   │       ├── Controllers/        # BoardsController
│   │       ├── Middleware/         # Exception, Security, Logging
│   │       ├── Extensions/         # Startup configuration
│   │       └── DTOs/               # Request DTOs
│   │
│   └── life-games-app/              # React Frontend
│       └── src/
│           ├── components/          # Board, Controls, Patterns
│           ├── api/                # boardsApi.ts
│           └── types/              # TypeScript interfaces
│
├── docs/                            # Documentation
│   ├── PRD.md                      # Product Requirements
│   ├── ARCHITECTURE.md             # Architecture deep dive
│   ├── PROJECT_SUMMARY.md          # Implementation details
│   ├── QUICK_START.md              # 2-minute guide
│   ├── POSTMAN_GUIDE.md            # API testing guide
│   └── DEPLOYMENT.md               # Deployment guide
│
├── postman/                         # API Testing
│   ├── Life-Games-API.postman_collection.json
│   └── Life-Games-API.postman_environment.json
│
├── CLAUDE.md                        # Project guidelines for Claude Code
└── AGENTS.md                        # Agent configuration
```

## 📚 API Documentation

### Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/boards` | Create new board with initial state |
| GET | `/api/boards/{id}` | Get board and generation 0 |
| GET | `/api/boards/{id}/next` | Compute next generation |
| GET | `/api/boards/{id}/generations/{n}` | Get specific generation (computed on demand) |
| GET | `/api/boards/{id}/final` | Detect final/stable state with cycle detection |
| GET | `/health` | Health check (includes DB connectivity) |

### Example: Create Board

```bash
POST http://localhost:5253/api/boards
Content-Type: application/json

{
  "name": "Glider",
  "cells": [
    { "x": 1, "y": 0 },
    { "x": 2, "y": 1 },
    { "x": 0, "y": 2 },
    { "x": 1, "y": 2 },
    { "x": 2, "y": 2 }
  ]
}
```

Response:
```json
{
  "boardId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "name": "Glider",
  "generationNumber": 0,
  "cells": [
    { "x": 1, "y": 0 },
    { "x": 2, "y": 1 },
    { "x": 0, "y": 2 },
    { "x": 1, "y": 2 },
    { "x": 2, "y": 2 }
  ],
  "createdAt": "2026-02-01T23:00:00Z",
  "links": {
    "self": { "href": "/api/boards/3fa85f64-5717-4562-b3fc-2c963f66afa6", "rel": "self" },
    "next": { "href": "/api/boards/3fa85f64-5717-4562-b3fc-2c963f66afa6/next", "rel": "next" },
    "final": { "href": "/api/boards/3fa85f64-5717-4562-b3fc-2c963f66afa6/final", "rel": "final" }
  }
}
```

### Postman Collection

Import the included Postman collection for instant API testing:

1. Open Postman
2. Import `postman/Life-Games-API.postman_collection.json`
3. Import `postman/Life-Games-API.postman_environment.json`
4. Select "Life Games - Local Development" environment
5. Run requests or entire collection with automated tests

See [docs/POSTMAN_GUIDE.md](docs/POSTMAN_GUIDE.md) for detailed instructions.

### Swagger UI

Interactive API documentation and testing:
```
http://localhost:5253/swagger
```

## 🏛️ Architecture Details

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────┐
│                        API Layer                            │
│  Controllers, Middleware, DTOs, Swagger                     │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                   Application Layer                         │
│  Handlers (CQRS), Services, Validators, DTOs                │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                 Infrastructure Layer                        │
│  Repositories, EF Core, DbContext, Configurations           │
└──────────────────────┬──────────────────────────────────────┘
                       │
┌──────────────────────▼──────────────────────────────────────┐
│                    Domain Layer                             │
│  Entities, Value Objects, Services, Interfaces              │
└─────────────────────────────────────────────────────────────┘
```

### Key Patterns

**Aggregate Pattern**
- `Board` is the aggregate root
- `BoardGeneration` is part of the aggregate
- Single repository (`IBoardRepository`) manages entire aggregate
- Ensures transactional consistency

**CQRS with MediatR**
- Commands: `CreateBoardCommand`
- Queries: `GetBoardQuery`, `GetGenerationQuery`, `GetNextGenerationQuery`, `GetFinalStateQuery`
- All handlers in single `Handlers/` folder

**Repository Pattern**
- No `IQueryable` leakage
- Returns domain objects, not EF Core entities
- Persistence ignorance maintained
- Easy to swap data stores

**Navigation Properties Optimization**
- Board includes Generations collection
- BoardGeneration includes Board reference
- 50% fewer database queries (1 query instead of 2)

See [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) for complete details.

## 🧪 Testing

```bash
cd src/life-games-api
dotnet test
```

**Test Coverage: 39 tests, all passing ✅**

| Layer | Tests | Coverage |
|-------|-------|----------|
| Domain | 13 | Game of Life rules, edge cases |
| Application | 6 | Cycle detection, business logic |
| Infrastructure | 12 | Repository operations, navigation properties |
| API | 8 | Integration tests, end-to-end scenarios |

### Test Categories

**Domain Tests** (`GameOfLifeEngineTests.cs`)
- Underpopulation (< 2 neighbors → dies)
- Overpopulation (> 3 neighbors → dies)
- Survival (2-3 neighbors → lives)
- Reproduction (exactly 3 neighbors → born)
- Edge cases (empty board, single cell)

**Application Tests** (`CycleDetectionServiceTests.cs`)
- Still life detection (Block pattern)
- Oscillator detection (Blinker, Toad)
- Cycle length calculation
- Max iterations handling

**Infrastructure Tests** (`BoardRepositoryTests.cs`)
- CRUD operations
- Navigation property loading
- Cascade deletes
- Query optimization

**API Tests** (`BoardsControllerIntegrationTests.cs`)
- HTTP status codes
- Request validation
- Response format
- HATEOAS links

## ⚙️ Configuration

### Backend (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=lifegames.db"
  },
  "Board": {
    "MaxIterationsForFinalState": 10000
  },
  "RateLimiting": {
    "PermitLimit": 1000,
    "WindowSeconds": 1
  },
  "Cors": {
    "AllowedOrigins": []
  }
}
```

### Environment-Specific Configuration

**Development** (`appsettings.Development.json`)
- CORS: Allow all origins
- Logging: Information level

**Production** (`appsettings.Production.json`)
- CORS: Specific origins only
- Logging: Warning level
- Trusted proxies and networks

### Frontend Configuration

**Vite Config** (`vite.config.ts`)
```typescript
export default defineConfig({
  server: {
    proxy: {
      '/api': 'http://localhost:5253'
    }
  }
})
```

## 💻 Development

### Backend Development

```bash
cd src/life-games-api

# Build
dotnet build

# Run tests
dotnet test

# Run with watch (auto-reload)
dotnet watch --project Api/LifeGames.Api

# Create migration
dotnet ef migrations add MigrationName --project Infrastructure/LifeGames.Infrastructure --startup-project Api/LifeGames.Api

# Apply migrations
dotnet ef database update --project Infrastructure/LifeGames.Infrastructure --startup-project Api/LifeGames.Api
```

### Frontend Development

```bash
cd src/life-games-app

# Install dependencies
npm install

# Run dev server (HMR enabled)
npm run dev

# Build for production
npm run build

# Preview production build
npm run preview

# Lint
npm run lint
```

### Database Management

**SQLite (Default)**
- File: `lifegames.db` in API project root
- No installation required
- Perfect for development and production
- Cells stored as JSON (TEXT column type)

## 🐳 Docker Deployment

### Build and Run

```bash
cd src/life-games-api
docker-compose up --build
```

### Docker Compose Services

```yaml
services:
  api:
    build: .
    ports:
      - "5253:8080"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
    healthcheck:
      test: ["CMD", "curl", "-f", "http://localhost:8080/health"]
```

### Multi-Stage Dockerfile

- Stage 1: Restore dependencies
- Stage 2: Build application
- Stage 3: Run tests
- Stage 4: Publish
- Stage 5: Runtime (minimal)

## ⚡ Performance

### Query Optimization

| Operation | Before | After | Improvement |
|-----------|--------|-------|-------------|
| Get Board | 2 queries | 1 query | 50% faster |
| Get Generation | 2 queries | 1 query | 50% faster |
| Get Next Generation | 2 queries | 1 query | 50% faster |

### Caching Strategy

- **Generation 0**: Always persisted (initial state)
- **Computed Generations**: Cached on first computation
- **Latest Generation**: Cached for incremental computation
- **Final State**: Computed on demand (not cached due to cycle detection)

### Rate Limiting

- **Default**: 1000 requests/second per IP
- **Window**: Fixed 1-second window
- **Rejection**: HTTP 429 (Too Many Requests)

## 🔧 Troubleshooting

### API won't start

**Issue**: Port 5253 already in use

**Solution**:
```bash
# Windows
netstat -ano | findstr :5253
taskkill /PID <process_id> /F

# Linux/Mac
lsof -ti:5253 | xargs kill -9
```

### Database connection fails

**Issue**: SQLite database file issues

**Solution**: Delete and recreate the database
```bash
# Delete existing database
rm lifegames.db

# Restart the API (database will be recreated)
dotnet run --project Api/LifeGames.Api
```

### Frontend API calls fail

**Issue**: CORS errors in browser console

**Solution**: Ensure backend is running and CORS is configured
```bash
# Check backend is running
curl http://localhost:5253/health
```

### Tests fail

**Issue**: Database locked or migration issues

**Solution**:
```bash
# Clean and rebuild
dotnet clean
dotnet build
dotnet test
```

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Make your changes
4. Run tests (`dotnet test` and `npm run lint`)
5. Commit your changes (`git commit -m 'Add amazing feature'`)
6. Push to the branch (`git push origin feature/amazing-feature`)
7. Open a Pull Request

### Code Style

- **C#**: Follow Microsoft's C# coding conventions
- **TypeScript**: ESLint configuration included
- **Naming**: PascalCase for public, camelCase for private

## 📖 Additional Documentation

- [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md) - Detailed architecture and design decisions
- [docs/PROJECT_SUMMARY.md](docs/PROJECT_SUMMARY.md) - Complete implementation summary
- [docs/QUICK_START.md](docs/QUICK_START.md) - 2-minute quick start guide
- [docs/POSTMAN_GUIDE.md](docs/POSTMAN_GUIDE.md) - API testing with Postman
- [docs/PRD.md](docs/PRD.md) - Product Requirements Document
- [docs/DEPLOYMENT.md](docs/DEPLOYMENT.md) - Deployment guide
- [CLAUDE.md](CLAUDE.md) - Project guidelines for Claude Code
- [AGENTS.md](AGENTS.md) - Agent configuration

## 📄 License

MIT License - see LICENSE file for details

## 🙏 Acknowledgments

- Built with [.NET 8](https://dotnet.microsoft.com/)
- UI powered by [React](https://react.dev/) and [Tailwind CSS](https://tailwindcss.com/)
- Inspired by [Conway's Game of Life](https://en.wikipedia.org/wiki/Conway%27s_Game_of_Life)

---

**Built with ❤️ using Clean Architecture and modern web technologies**
