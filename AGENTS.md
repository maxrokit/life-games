# AI Agents Guide - Conway's Game of Life Project

This document provides comprehensive context for AI agents analyzing or modifying this project.

## Project Overview

**Name**: Conway's Game of Life - Full Stack Application
**Type**: Production-ready REST API + React Frontend
**Status**: ✅ Complete and Production Ready
**Created**: February 2026
**Architecture**: Clean Architecture with CQRS
**Tech Stack**: .NET 8, React 18, TypeScript, SQLite, AWS Integration

## Quick Context

This is a **complete, production-ready implementation** of Conway's Game of Life with:
- RESTful API following Clean Architecture principles
- React + TypeScript frontend with dual-mode operation
- Comprehensive testing (36 tests, 100% passing)
- AWS integration (CloudWatch, Parameter Store)
- Full Docker support
- Complete documentation

## Project Structure

```
life-games/
├── src/
│   ├── life-games-api/              # .NET 8 Backend API
│   ├── Domain/                  # Core business logic (no dependencies)
│   │   ├── LifeGames.Domain/
│   │   │   ├── Entities/        # Board, BoardGeneration
│   │   │   ├── ValueObjects/    # Cell (coordinates)
│   │   │   ├── Services/        # GameOfLifeEngine (Conway's rules)
│   │   │   └── Interfaces/      # IBoardRepository, etc.
│   │   └── LifeGames.Domain.Tests/ (13 tests)
│   │
│   ├── Application/             # Use cases (depends on Domain)
│   │   ├── LifeGames.Application/
│   │   │   ├── Handlers/        # MediatR handlers (CreateBoardCommand, GetBoard, GetNextGeneration, GetGeneration, GetFinalState)
│   │   │   ├── Services/        # CycleDetectionService
│   │   │   ├── Validators/      # FluentValidation rules
│   │   │   ├── DTOs/            # Data transfer objects
│   │   │   ├── Behaviors/       # ValidationBehavior (MediatR pipeline)
│   │   │   └── Options/         # BoardOptions (configuration)
│   │   └── LifeGames.Application.Tests/ (6 tests)
│   │
│   ├── Infrastructure/          # Data access (depends on Domain, Application)
│   │   ├── LifeGames.Infrastructure/
│   │   │   ├── Data/            # LifeGamesDbContext (EF Core)
│   │   │   ├── Configurations/  # EF Core entity configurations
│   │   │   └── Repositories/    # BoardRepository, BoardGenerationRepository
│   │   └── LifeGames.Infrastructure.Tests/ (9 tests)
│   │
│   ├── Api/                     # REST API (depends on all layers)
│   │   ├── LifeGames.Api/
│   │   │   ├── Controllers/     # BoardsController
│   │   │   ├── DTOs/            # CreateBoardRequest
│   │   │   ├── Middleware/      # ExceptionHandlingMiddleware
│   │   │   ├── Program.cs       # DI, middleware pipeline, AWS config
│   │   │   ├── appsettings.json
│   │   │   ├── appsettings.Production.json
│   │   │   └── Dockerfile
│   │   └── LifeGames.Api.Tests/ (8 tests)
│   │
│   ├── docker-compose.yml
│   └── LifeGames.sln
│
│   └── life-games-app/              # React Frontend
│   ├── src/
│   │   ├── components/
│   │   │   ├── Board.tsx        # SVG grid visualization
│   │   │   ├── Controls.tsx     # Step/Run/Pause/Reset/Clear
│   │   │   └── Patterns.tsx     # 7 preset patterns
│   │   ├── api/
│   │   │   └── boardsApi.ts     # API client (fetch-based)
│   │   ├── types/
│   │   │   └── index.ts         # TypeScript interfaces
│   │   ├── App.tsx              # Main app (dual-mode logic)
│   │   ├── main.tsx
│   │   └── index.css
│   ├── vite.config.ts           # Vite + Tailwind + API proxy
│   └── package.json
│
├── docs/
│   ├── PRD.md                   # Product requirements (updated)
│   ├── SECRETS.md               # Secrets management guide
│   └── LOGGING.md               # Logging configuration guide
│
├── .memory/
│   └── implementation-notes.md  # Technical decisions
│
├── README.md                    # Project overview
├── PROJECT_SUMMARY.md           # Complete implementation details
├── QUICK_START.md               # 2-minute quick start
├── CLAUDE.md                    # Development guide
└── AGENTS.md                    # This file
```

## Architecture Deep Dive

### Clean Architecture Layers

**Dependency Rule**: Domain ← Application ← Infrastructure ← API

1. **Domain Layer** (Core)
   - No external dependencies
   - Pure C# business logic
   - Entities: `Board`, `BoardGeneration`
   - Value Objects: `Cell` (readonly record struct)
   - Services: `GameOfLifeEngine` (static, pure functions)
   - Interfaces: Define repository contracts

2. **Application Layer** (Use Cases)
   - Depends on: Domain
   - CQRS with MediatR
   - Commands: Create operations
   - Queries: Read operations
   - Validators: FluentValidation
   - Pipeline: ValidationBehavior intercepts all requests

3. **Infrastructure Layer** (Data)
   - Depends on: Domain, Application
   - EF Core 8 with SQLite
   - JSON column storage for sparse cells
   - Repositories implement domain interfaces
   - No IQueryable leakage (returns domain entities)

4. **API Layer** (Entry Point)
   - Depends on: All layers
   - ASP.NET Core Web API
   - Controllers delegate to MediatR
   - Middleware: Exception handling, logging
   - Configuration: DI, AWS, Serilog, versioning

### Key Design Patterns

- **CQRS**: Commands vs Queries separation
- **Repository**: Abstracts data access
- **Options Pattern**: Strongly-typed configuration
- **Mediator**: MediatR decouples controllers from handlers
- **Pipeline Behavior**: Cross-cutting concerns (validation)
- **HATEOAS**: Hypermedia links in responses

## Production Hardening Features

### Security Middleware

**CorrelationIdMiddleware** (`Api/LifeGames.Api/Middleware/CorrelationIdMiddleware.cs`)
- Adds `X-Correlation-ID` header to all requests/responses
- Generates new ID if not provided by client
- Injects correlation ID into Serilog log context for request tracing

**SecurityHeadersMiddleware** (`Api/LifeGames.Api/Middleware/SecurityHeadersMiddleware.cs`)
- HSTS (HTTP Strict Transport Security) - Force HTTPS for 1 year
- X-Content-Type-Options: nosniff - Prevent MIME-sniffing attacks
- X-Frame-Options: DENY - Prevent clickjacking
- X-XSS-Protection: enabled - Browser XSS protection
- Referrer-Policy: strict-origin-when-cross-origin
- Content-Security-Policy: Restrictive for API (no inline scripts)
- Permissions-Policy: Restrict browser features (geolocation, camera, etc.)

### Configuration Options

**RateLimitingOptions** (`Api/LifeGames.Api/Options/RateLimitingOptions.cs`)
```csharp
{
  "RateLimiting": {
    "PermitLimit": 100,      // Max requests per window
    "WindowSeconds": 60       // Time window in seconds
  }
}
```

### Request/Response Features

- **Request Size Limits**: 10MB max (prevents DoS attacks)
- **Response Caching**: GET endpoints cached (30-60 seconds)
- **CORS**: Environment-specific (AllowAny in dev, configured origins in prod)
- **Forwarded Headers**: Proper client IP tracking behind proxies/load balancers
- **Rate Limiting**: Configurable per IP address

### Database Migrations

- **EF Core Migrations**: Created with `dotnet ef migrations add InitialCreate`
- **Migration Strategy**:
  - In-memory databases (tests): `EnsureCreated()`
  - Physical databases with migrations: `MigrateAsync()`
  - Graceful handling of already-created databases
  - Production requires migrations (throws if missing)

**Migration Location**: `Infrastructure/LifeGames.Infrastructure/Migrations/`

## Critical Implementation Details

### Game of Life Algorithm

**Location**: `Domain/LifeGames.Domain/Services/GameOfLifeEngine.cs`

**Algorithm**:
1. Build set of all cells to check (alive cells + their 8 neighbors)
2. For each cell, count living neighbors
3. Apply Conway's rules:
   - Birth: Dead cell with exactly 3 neighbors becomes alive
   - Survival: Alive cell with 2-3 neighbors survives
   - Death: All other cells die/stay dead
4. Return new set of living cells

**Key Feature**: Sparse representation (only stores alive cells)

**Complexity**: O(n) where n = number of alive cells + their neighbors

### Cycle Detection

**Location**: `Application/LifeGames.Application/Services/CycleDetectionService.cs`

**Algorithm**:
1. Maintain history of board states (serialized cell coordinates → generation number)
2. Compute next generation
3. If state matches previous state: cycle detected
4. If no match after max iterations: no cycle found

**Detects**:
- Still lifes: Cycle length = 1 (state doesn't change)
- Oscillators: Cycle length > 1 (repeating pattern)

**Max Iterations**: 10,000 (configurable via `BoardOptions`)

### Data Storage

**Database**: SQLite (EF Core 8)

**Schema**:
```sql
CREATE TABLE Boards (
    Id TEXT PRIMARY KEY,           -- Guid
    Name TEXT,
    CreatedAt TEXT                 -- ISO 8601
);

CREATE TABLE BoardGenerations (
    Id TEXT PRIMARY KEY,           -- Guid
    BoardId TEXT NOT NULL,
    GenerationNumber INTEGER NOT NULL,
    Cells TEXT NOT NULL,           -- JSON: [{x:0,y:0},{x:1,y:0}]
    ComputedAt TEXT,
    UNIQUE(BoardId, GenerationNumber),
    FOREIGN KEY(BoardId) REFERENCES Boards(Id) ON DELETE CASCADE
);
```

**Storage Strategy**: Sparse cell storage (JSON column)
- Only alive cell coordinates stored
- Efficient for large, sparse boards
- Example: `[{"x":0,"y":0},{"x":1,"y":0},{"x":0,"y":1}]`

## API Endpoints Reference

| Method | Endpoint | Handler | Description |
|--------|----------|---------|-------------|
| POST | `/api/boards` | CreateBoardCommand | Create board, returns HATEOAS links |
| GET | `/api/boards/{id}` | GetBoardQuery | Get generation 0 (initial state) |
| GET | `/api/boards/{id}/next` | GetNextGenerationQuery | Get next generation (N+1) |
| GET | `/api/boards/{id}/generations/{n}` | GetGenerationQuery | Get specific generation N |
| GET | `/api/boards/{id}/final` | GetFinalStateQuery | Get final state with cycle info |
| GET | `/health` | - | Health check |
| GET | `/health/ready` | - | Readiness check (DB connectivity) |

### API Versioning

**Method**: Media type versioning (per PRD requirement)

**Accept Headers**:
- `application/vnd.lifegames.v1+json` → Version 1, JSON
- `application/vnd.lifegames.v1+xml` → Version 1, XML
- `application/json` → Defaults to v1, JSON
- No header → Defaults to v1, JSON

**Configuration**: `Program.cs` lines 34-49

### HATEOAS Links

All responses include `_links` section:
```json
{
  "id": "123e4567-...",
  "cells": [...],
  "_links": {
    "self": { "href": "/api/boards/123..." },
    "next": { "href": "/api/boards/123.../next" },
    "final": { "href": "/api/boards/123.../final" }
  }
}
```

## Configuration & Secrets

### Development (Local)

**User Secrets**: `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Data Source=lifegames.db"`

**User Secrets ID**: `6619146d-dff1-41fa-b8ed-37d8772129c1`

**Location**: `%APPDATA%\Microsoft\UserSecrets\6619146d-dff1-41fa-b8ed-37d8772129c1\secrets.json`

### Production (AWS)

**Secrets**: AWS Systems Manager Parameter Store
- Path: `/lifegames/` (configurable)
- Example: `/lifegames/ConnectionStrings__DefaultConnection`

**Logging**: AWS CloudWatch Logs
- Log Group: `/aws/lifegames/api`
- Format: Compact JSON
- Batch size: 100 events per 10 seconds

**IAM Permissions Required**:
```json
{
  "Statement": [
    {
      "Effect": "Allow",
      "Action": ["ssm:GetParameter*", "logs:CreateLogGroup", "logs:CreateLogStream", "logs:PutLogEvents"],
      "Resource": ["arn:aws:ssm:*:*:parameter/lifegames/*", "arn:aws:logs:*:*:log-group:/aws/lifegames/*"]
    }
  ]
}
```

## Testing Strategy

### Test Coverage (36 tests total)

1. **Domain Tests** (13 tests) - `LifeGames.Domain.Tests`
   - Still lifes: Block, Beehive
   - Oscillators: Blinker, Toad
   - Spaceships: Glider
   - Edge cases: Empty board, single cell, large boards
   - Cancellation token support

2. **Application Tests** (6 tests) - `LifeGames.Application.Tests`
   - Cycle detection for empty boards
   - Cycle detection for stable patterns
   - Cycle detection for oscillators
   - Max iteration handling
   - Cancellation support

3. **Infrastructure Tests** (9 tests) - `LifeGames.Infrastructure.Tests`
   - Board CRUD operations
   - Generation storage and retrieval
   - Cascade deletes
   - In-memory SQLite for isolation

4. **API Tests** (8 tests) - `LifeGames.Api.Tests`
   - All endpoint integration tests
   - Error scenarios (404 Not Found)
   - Health checks
   - Uses WebApplicationFactory for in-memory testing

### Running Tests

```bash
cd src/life-games-api
dotnet test                    # All 36 tests
dotnet test --verbosity normal # Detailed output
dotnet test --filter "FullyQualifiedName~Domain"  # Domain tests only
```

## Frontend Architecture

### Dual-Mode Operation

**Local Mode**: Pure client-side simulation
- Game of Life logic in `App.tsx` (lines 22-54)
- No backend required
- Works offline

**API Mode**: Server-side computation
- Calls backend API for each generation
- Creates board on pattern selection
- Follows HATEOAS links

### Components

1. **Board.tsx** (SVG Grid)
   - Auto-scaling viewport
   - Click-to-toggle cells
   - Grid lines for reference
   - Green cells (#22c55e) on dark background

2. **Controls.tsx** (UI Controls)
   - Step: Single generation
   - Run: Auto-advance (interval-based)
   - Pause: Stop auto-advance
   - Reset: Back to generation 0
   - Clear: Empty board
   - Speed slider: 50ms - 1000ms

3. **Patterns.tsx** (Presets)
   - Glider (spaceship)
   - Blinker (period 2 oscillator)
   - Block (still life)
   - Beehive (still life)
   - Toad (period 2 oscillator)
   - Pulsar (period 3 oscillator)
   - Gosper Glider Gun (infinite growth)

### State Management

- **useState**: cells, boardId, generation, isRunning, speed, loading, error, useApi
- **useRef**: initialCellsRef (for reset), intervalRef (for auto-run timer)
- **useCallback**: Memoized functions (computeNextGeneration, step, run, pause, etc.)
- **useEffect**: Auto-run interval management

## Common Modification Scenarios

### Adding a New API Endpoint

1. **Create Handler** in `Application/LifeGames.Application/Handlers/` with query/command record and handler class implementing `IRequestHandler<TRequest, TResponse>`
2. **Add Validator** in `Application/LifeGames.Application/Validators/`
3. **Add Controller Action** in `Api/LifeGames.Api/Controllers/BoardsController.cs`
4. **Add Integration Test** in `Api/LifeGames.Api.Tests/`

### Adding a New Pattern

Edit `life-games-app/src/components/Patterns.tsx`:
```typescript
const patterns = [
  // ... existing patterns
  {
    name: 'New Pattern',
    cells: [
      { x: 0, y: 0 },
      { x: 1, y: 0 },
      // ... more cells
    ]
  }
];
```

### Changing Board Size Limits

Edit `life-games-api/Api/LifeGames.Api/appsettings.json`:
```json
{
  "Board": {
    "MaxWidth": 2000,    // was 1000
    "MaxHeight": 2000,   // was 1000
    "MaxIterationsForFinalState": 20000  // was 10000
  }
}
```

### Changing Database Provider

1. Replace `Microsoft.EntityFrameworkCore.Sqlite` package
2. Update connection string in User Secrets / Parameter Store
3. Update `Infrastructure/LifeGames.Infrastructure/DependencyInjection.cs`
4. Run migrations: `dotnet ef migrations add InitialCreate`

## Build & Deployment

### Local Development

```bash
# Backend
cd src/life-games-api
dotnet restore
dotnet build
dotnet run --project Api/LifeGames.Api
# → http://localhost:5000 (API)
# → http://localhost:5000/swagger (Docs)

# Frontend
cd src/life-games-app
npm install
npm run dev
# → http://localhost:5173
```

### Docker

```bash
cd src/life-games-api
docker-compose up --build
# → http://localhost:5000
```

### Production Build

```bash
# Backend
cd src/life-games-api
dotnet publish -c Release -o ./publish

# Frontend
cd src/life-games-app
npm run build  # Output in dist/
```

## Code Quality Standards

### Current Metrics

- **Build**: 0 warnings, 0 errors
- **Tests**: 36/36 passing (100%)
- **Linting**: 0 ESLint errors/warnings
- **TypeScript**: Strict mode enabled
- **C#**: Warning level 5, .NET analyzers enabled

### Coding Conventions

**C#**:
- Clean Architecture: Respect layer dependencies
- No IQueryable leakage from repositories
- Async/await throughout (CancellationToken propagation)
- FluentValidation for all inputs
- Structured logging with named properties

**TypeScript**:
- Strict mode enabled
- No implicit any
- Prefer functional components with hooks
- Memoize expensive computations (useMemo, useCallback)
- Avoid unused variables

## Troubleshooting Guide

### Backend Won't Start

1. Check port 5000/5001 availability
2. Verify User Secrets: `dotnet user-secrets list`
3. Check database file permissions
4. Validate appsettings.json syntax

### Frontend Build Errors

1. Run `npm install` to restore dependencies
2. Check TypeScript errors: `npm run build`
3. Lint check: `npm run lint`

### Tests Failing

1. Clean and rebuild: `dotnet clean && dotnet build`
2. Delete bin/obj folders: `find . -type d -name "obj" -exec rm -rf {} +`
3. Restore packages: `dotnet restore`

### API Integration Not Working

1. Verify backend running on port 5000
2. Check CORS settings in Program.cs
3. Inspect browser network tab for errors
4. Check User Secrets configuration

## Performance Characteristics

- **API Response Time**: <10ms for typical patterns (Glider, Blinker)
- **Large Boards**: ~50-100ms for 1000+ cells
- **Database Queries**: Sub-millisecond with indexes
- **Frontend Render**: <16ms (60fps) for boards <500 cells
- **Cycle Detection**: <1 second for most patterns (<10,000 iterations)

## Security Considerations

**Implemented**:
- ✅ Input validation (FluentValidation)
- ✅ Rate limiting (100 req/min per IP)
- ✅ CORS configured
- ✅ SQL injection prevention (EF Core parameterization)
- ✅ Secrets management (User Secrets + AWS Parameter Store)
- ✅ Structured logging (no sensitive data)

**Not Implemented** (per PRD non-goals):
- ❌ Authentication/Authorization
- ❌ HTTPS enforcement (configured but not enforced in dev)

**Production Recommendations**:
- Add authentication/authorization
- Enable HTTPS enforcement
- Update CORS to specific origins (never AllowAnyOrigin)
- Configure rate limiting per user/endpoint
- Add request/response size limits

## Package Versions (Critical)

### Backend (.NET 8)
- Microsoft.EntityFrameworkCore.Sqlite: 8.0.11
- Microsoft.EntityFrameworkCore.Design: 8.0.11 (migrations)
- MediatR: 14.0.0
- FluentValidation.AspNetCore: 12.1.1
- Serilog.AspNetCore: 8.0.3
- Swashbuckle.AspNetCore: 6.9.0
- Asp.Versioning.Mvc: 8.1.0
- Amazon.Extensions.Configuration.SystemsManager: 7.0.1
- Serilog.Sinks.AwsCloudWatch: 4.4.42

### Frontend
- React: 19.2.0
- TypeScript: 5.9.3
- Vite: 7.2.4
- @tailwindcss/vite: 4.1.18

## Known Limitations

1. **Board Size**: Limited to configured max (default 1000x1000)
2. **Iteration Limit**: Final state detection stops at 10,000 generations
3. **No Persistence in Frontend**: Local mode doesn't save state
4. **Single User**: No multi-tenancy or user accounts
5. **No Board Search**: Cannot list or search existing boards
6. **Memory Growth**: Long-running patterns cache all generations

## Important Files for Agents

### Must Read First
1. `README.md` - Project overview
2. `QUICK_START.md` - Getting started
3. `docs/PRD.md` - Requirements (with implementation status)

### Architecture Reference
4. `PROJECT_SUMMARY.md` - Complete implementation details
5. `.memory/implementation-notes.md` - Technical decisions
6. `AGENTS.md` - This file

### Operational Guides
7. `docs/SECRETS.md` - Secrets management
8. `docs/LOGGING.md` - Logging configuration

### Code Entry Points
9. `life-games-api/Api/LifeGames.Api/Program.cs` - Backend startup
10. `life-games-app/src/App.tsx` - Frontend main component
11. `Domain/LifeGames.Domain/Services/GameOfLifeEngine.cs` - Core algorithm

## Questions Agents Should Ask

Before making changes:
1. **Which layer does this change belong to?** (Respect Clean Architecture)
2. **Are there existing tests to modify?** (Maintain 100% pass rate)
3. **Does this break API versioning?** (Media type versioning required)
4. **Does this expose IQueryable?** (Not allowed per architecture)
5. **Are secrets hardcoded?** (Use User Secrets or Parameter Store)
6. **Is logging structured?** (Use named properties, not string interpolation)
7. **Is cancellation supported?** (CancellationToken throughout)

## Common Pitfalls to Avoid

1. ❌ **Breaking Clean Architecture**: Don't add Domain dependencies on Application/Infrastructure
2. ❌ **Leaking IQueryable**: Repositories must return domain entities
3. ❌ **Hardcoding Secrets**: Use User Secrets or environment variables
4. ❌ **String Interpolation in Logging**: Use structured logging with named properties
5. ❌ **Forgetting Cancellation Tokens**: Propagate through all async methods
6. ❌ **Skipping Validation**: All inputs must have FluentValidation rules
7. ❌ **Breaking Tests**: Maintain 100% pass rate (36/36)

## Agent Success Metrics

When working on this codebase, ensure:
- ✅ All 36 tests passing
- ✅ 0 compiler warnings
- ✅ 0 linter errors
- ✅ Clean Architecture respected
- ✅ Documentation updated
- ✅ No secrets hardcoded
- ✅ Structured logging maintained

---

**Last Updated**: February 1, 2026
**Status**: Production Ready
**Maintainer**: See git history
**Support**: See documentation in `docs/` directory
