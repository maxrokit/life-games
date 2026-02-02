# Conway's Game of Life - Full Stack Application

Production-ready implementation of Conway's Game of Life with .NET 8 backend API and React frontend.

## Project Structure

```
life-games/
├── src/
│   ├── life-games-api/      # .NET 8 API (Clean Architecture)
│   │   ├── Domain/          # Core business logic
│   │   ├── Application/     # Use cases (CQRS with MediatR)
│   │   ├── Infrastructure/  # EF Core + SQL Server
│   │   └── Api/             # REST API controllers
│   │
│   └── life-games-app/      # React + TypeScript frontend
│       └── src/
│           ├── components/  # React components
│           ├── api/        # API client
│           └── types/      # TypeScript types
│
└── docs/
    └── PRD.md              # Product Requirements Document
```

## Quick Start

### Backend API

```bash
cd src/life-games-api
dotnet restore
dotnet build
dotnet run --project Api/LifeGames.Api

# API available at: http://localhost:5253
# Swagger UI: http://localhost:5253/swagger
```

### Frontend

```bash
cd src/life-games-app
npm install
npm run dev

# App available at: http://localhost:5173
```

### With Docker

```bash
cd src/life-games-api
docker-compose up --build

# API available at: http://localhost:5253
```

## Features

### Backend API
- RESTful API with HATEOAS links
- Clean Architecture with CQRS pattern
- SQL Server persistence with EF Core (LocalDB for development)
- Cycle detection for oscillators and still lifes
- API versioning and content negotiation (JSON/XML)
- Rate limiting and response caching
- FluentValidation for input validation
- Serilog structured logging
- Health checks
- Swagger/OpenAPI documentation

### Frontend
- Interactive grid visualization
- Click-to-toggle cells
- Preset patterns (Glider, Blinker, Gosper Gun, etc.)
- Local simulation mode (pure client-side)
- API mode (server-side computation)
- Auto-run with adjustable speed
- Generation counter and cell count
- Responsive design with Tailwind CSS

## API Endpoints

- `POST /api/boards` - Create new board
- `GET /api/boards/{id}` - Get board (generation 0)
- `GET /api/boards/{id}/next` - Get next generation
- `GET /api/boards/{id}/generations/{n}` - Get specific generation
- `GET /api/boards/{id}/final` - Detect final/stable state
- `GET /health` - Health check

## API Testing

### Postman Collection

Ready-to-use Postman collection with all endpoints, examples, and automated tests:

```bash
# Import into Postman:
# 1. Open Postman
# 2. Import both files:
#    - Life-Games-API.postman_collection.json
#    - Life-Games-API.postman_environment.json
# 3. Select "Life Games - Local Development" environment

# See POSTMAN_GUIDE.md for detailed usage instructions
```

Collection includes:
- All API endpoints with example requests
- Common Game of Life patterns (Glider, Blinker, Pulsar, Block)
- Automated tests for status codes and response validation
- Content negotiation examples (JSON/XML)
- Error handling scenarios
- Health check endpoints

### Swagger UI

Interactive API documentation:
```
http://localhost:5253/swagger
```

## Tech Stack

### Backend
- .NET 8.0
- ASP.NET Core Web API
- Entity Framework Core 8.0
- SQL Server (LocalDB for development)
- MediatR (CQRS)
- FluentValidation
- Serilog
- Swashbuckle (Swagger)

### Frontend
- React 18
- TypeScript
- Vite
- Tailwind CSS v4
- Fetch API

## Architecture

The backend follows **Clean Architecture** with **Aggregate Pattern**:

- **Domain Layer**: Pure business logic (Game of Life rules)
  - Board entity (aggregate root) with Generations collection
  - BoardGeneration entity with Board navigation property
  - Single repository interface: IBoardRepository
- **Application Layer**: Use cases with MediatR handlers
  - All handlers in Handlers/ folder (CQRS pattern)
  - Query optimization using navigation properties (1 DB call instead of 2)
- **Infrastructure Layer**: Database, repositories, logging
  - Single BoardRepository managing the Board aggregate
  - EF Core with navigation properties configured
- **API Layer**: Controllers, middleware, DTOs

## Testing

```bash
# Backend - 39 tests
cd src/life-games-api
dotnet test

# Results:
# - Domain: 13 tests (Game of Life rules)
# - Application: 6 tests (Cycle detection)
# - Infrastructure: 12 tests (Repository with navigation properties)
# - API: 8 tests (Integration tests)
```

## Configuration

### Backend (`appsettings.json`)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=lifegames.db"
  },
  "Board": {
    "MaxWidth": 1000,
    "MaxHeight": 1000,
    "MaxIterationsForFinalState": 10000
  }
}
```

### Frontend (`vite.config.ts`)

The frontend proxies API requests to `http://localhost:5253`.

## Development Workflow

1. Start the backend API
2. Start the frontend dev server
3. Open browser to http://localhost:5173
4. Toggle "Use API" to switch between local/API mode

## License

MIT
