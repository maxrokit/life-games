# Implementation Plan: Conway's Game of Life API

## Overview
Build a production-ready RESTful API for Conway's Game of Life using .NET 8, Clean Architecture, and SQLite with EF Core.

## Phase 1: Project Structure & Foundation

### 1.1 Create Solution and Projects
```
life-games/
├── life-games-api/
│   ├── LifeGames.sln
│   ├── Domain/
│   │   ├── LifeGames.Domain/
│   │   └── LifeGames.Domain.Tests/
│   ├── Application/
│   │   ├── LifeGames.Application/
│   │   └── LifeGames.Application.Tests/
│   ├── Infrastructure/
│   │   ├── LifeGames.Infrastructure/
│   │   └── LifeGames.Infrastructure.Tests/
│   └── Api/
│       ├── LifeGames.Api/
│       └── LifeGames.Api.Tests/
├── life-games-app/  (Phase 5 - later)
└── docs/
    └── PRD.md
```

### 1.2 NuGet Packages
- **Domain**: None (pure C#)
- **Application**: MediatR, FluentValidation
- **Infrastructure**: EF Core, EF Core SQLite, Serilog sinks
- **Api**: Asp.Versioning.Mvc, Swashbuckle, Serilog.AspNetCore, FluentValidation.AspNetCore

---

## Phase 2: Domain Layer

### 2.1 Entities
- `Board` - Id (Guid), CreatedAt, Name (optional)
- `BoardGeneration` - Id, BoardId, GenerationNumber, Cells (sparse coordinates), ComputedAt
- `Cell` - value object (X, Y coordinates)

### 2.2 Domain Logic
- `GameOfLifeEngine` - Core algorithm for computing next generation
  - Apply Conway's rules (birth: 3 neighbors, survival: 2-3 neighbors)
  - Input/output: sparse cell collections
  - Support cancellation token for long computations

### 2.3 Interfaces (defined in Domain)
- `IBoardRepository` - CRUD for boards, no IQueryable exposure
- `IBoardGenerationRepository` - Store/retrieve generations

### 2.4 Unit Tests
- Game of Life rules (still lifes, oscillators, spaceships)
- Edge cases (empty board, single cell, max size)

---

## Phase 3: Application Layer

### 3.1 Configuration Options
```csharp
public class BoardOptions
{
    public int MaxWidth { get; set; } = 1000;
    public int MaxHeight { get; set; } = 1000;
    public int MaxIterationsForFinalState { get; set; } = 10000;
}

public class RateLimitOptions { ... }
```

### 3.2 MediatR Commands/Queries (CQRS)
- `CreateBoardCommand` → returns BoardId + HATEOAS links
- `GetBoardQuery` → returns board state
- `GetNextGenerationQuery` → compute/cache generation N+1
- `GetGenerationQuery` → compute/cache generation N
- `GetFinalStateQuery` → detect cycle or stable state

### 3.3 Services
- `IGameOfLifeService` - Orchestrates generation computation
- `ICycleDetectionService` - Floyd's or hash-based cycle detection

### 3.4 Validators (FluentValidation)
- Board size validation (within configured max)
- Generation number validation (non-negative)
- Cell coordinate validation

### 3.5 Unit Tests
- Handler logic with mocked repositories
- Validation rules
- Cycle detection algorithm

---

## Phase 4: Infrastructure Layer

### 4.1 EF Core Setup
- `LifeGamesDbContext`
- Board entity configuration (JSON column for metadata)
- BoardGeneration entity configuration (JSON column for cells)
- SQLite connection string from configuration/secrets

### 4.2 Repositories
- `BoardRepository : IBoardRepository`
- `BoardGenerationRepository : IBoardGenerationRepository`
- All methods accept CancellationToken

### 4.3 Logging
- Serilog configuration (Console + CloudWatch sink)
- Structured logging with correlation IDs

### 4.4 Unit/Integration Tests
- Repository tests with in-memory SQLite
- EF Core mapping tests

---

## Phase 5: API Layer

### 5.1 Controllers
```
POST   /api/boards              → Create board
GET    /api/boards/{id}         → Get board (generation 0)
GET    /api/boards/{id}/next    → Get next generation
GET    /api/boards/{id}/generations/{n}  → Get generation N
GET    /api/boards/{id}/final   → Get final/stable state
```

### 5.2 DTOs & HATEOAS
- `BoardResponseDto` with `_links` section
- `GenerationResponseDto` with state + generation number
- `FinalStateResponseDto` with cycle info if applicable

### 5.3 Middleware & Configuration
- Global exception handler → Problem Details (RFC 7807)
- API versioning (media type, default v1/JSON)
- Content negotiation (JSON/XML)
- Rate limiting
- Response caching
- Health checks (`/health`, `/health/ready`)
- Swagger/OpenAPI

### 5.4 Program.cs Setup
- Options pattern registration with validation
- MediatR registration
- FluentValidation registration
- EF Core registration
- Serilog configuration
- CORS for frontend

### 5.5 Integration Tests
- Full API endpoint tests
- Rate limiting tests
- Content negotiation tests

---

## Phase 6: Docker & Deployment

### 6.1 Dockerfile
- Multi-stage build
- .NET 8 runtime image
- Health check instruction

### 6.2 docker-compose.yml (local dev)
- API service
- Volume for SQLite persistence

### 6.3 AWS Configuration
- Secrets Manager integration
- CloudWatch logging sink
- ECS/Fargate ready

---

## Phase 7: React Frontend (Secondary Goal)

### 7.1 Setup
- Vite + React + TypeScript
- Tailwind CSS

### 7.2 Components
- Board grid visualization
- Controls (step, run, pause, reset)
- Board upload/create
- Generation navigation

---

## Files to Create/Modify

### Phase 1-2 (Domain)
- `life-games-api/LifeGames.sln`
- `life-games-api/Domain/LifeGames.Domain/LifeGames.Domain.csproj`
- `life-games-api/Domain/LifeGames.Domain/Entities/Board.cs`
- `life-games-api/Domain/LifeGames.Domain/Entities/BoardGeneration.cs`
- `life-games-api/Domain/LifeGames.Domain/ValueObjects/Cell.cs`
- `life-games-api/Domain/LifeGames.Domain/Services/GameOfLifeEngine.cs`
- `life-games-api/Domain/LifeGames.Domain/Interfaces/IBoardRepository.cs`
- `life-games-api/Domain/LifeGames.Domain.Tests/GameOfLifeEngineTests.cs`

### Phase 3 (Application)
- `life-games-api/Application/LifeGames.Application/Commands/CreateBoardCommand.cs`
- `life-games-api/Application/LifeGames.Application/Queries/GetGenerationQuery.cs`
- `life-games-api/Application/LifeGames.Application/Queries/GetFinalStateQuery.cs`
- `life-games-api/Application/LifeGames.Application/Services/CycleDetectionService.cs`
- `life-games-api/Application/LifeGames.Application/Options/BoardOptions.cs`
- `life-games-api/Application/LifeGames.Application/Validators/CreateBoardValidator.cs`

### Phase 4 (Infrastructure)
- `life-games-api/Infrastructure/LifeGames.Infrastructure/Data/LifeGamesDbContext.cs`
- `life-games-api/Infrastructure/LifeGames.Infrastructure/Repositories/BoardRepository.cs`
- `life-games-api/Infrastructure/LifeGames.Infrastructure/Logging/SerilogConfiguration.cs`

### Phase 5 (API)
- `life-games-api/Api/LifeGames.Api/Controllers/BoardsController.cs`
- `life-games-api/Api/LifeGames.Api/Middleware/ExceptionHandlingMiddleware.cs`
- `life-games-api/Api/LifeGames.Api/Program.cs`
- `life-games-api/Api/LifeGames.Api/appsettings.json`
- `life-games-api/Api/LifeGames.Api/Dockerfile`

---

## Verification

### Unit Tests
```bash
cd life-games-api
dotnet test
```

### Run Locally
```bash
cd life-games-api/Api/LifeGames.Api
dotnet run
# Swagger: https://localhost:5001/swagger
```

### Docker
```bash
cd life-games-api
docker build -t life-games-api .
docker run -p 5000:8080 life-games-api
```

### Manual API Testing
1. Create board: `POST /api/boards` with cell data
2. Verify HATEOAS links in response
3. Get next state: `GET /api/boards/{id}/next`
4. Test cycle detection with known oscillator (blinker)
5. Test rate limiting by rapid requests
6. Test content negotiation with `Accept: application/xml`

---

## Implementation Order

1. **Phase 1**: Project structure (30 min)
2. **Phase 2**: Domain layer + tests (2-3 hrs)
3. **Phase 3**: Application layer + tests (2-3 hrs)
4. **Phase 4**: Infrastructure layer + tests (2 hrs)
5. **Phase 5**: API layer + tests (3-4 hrs)
6. **Phase 6**: Docker (1 hr)
7. **Phase 7**: Frontend (separate effort)

Start with Phase 1?
