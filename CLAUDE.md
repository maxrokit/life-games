# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Conway's Game of Life RESTful API with React/Tailwind frontend. See `docs/PRD.md` for full requirements.

## Build & Test Commands

```bash
# API (from life-games-api/)
dotnet build
dotnet test
dotnet run --project Api/LifeGames.Api

# Frontend (from life-games-app/)
npm install
npm run dev
npm run build
```

## Architecture

**Clean Architecture** with CQRS pattern (MediatR):

```
life-games-api/
├── Domain/LifeGames.Domain/        # Entities, interfaces, Game of Life logic
├── Application/LifeGames.Application/  # Use cases, handlers (commands & queries)
│   ├── Handlers/                   # MediatR command and query handlers
│   ├── DTOs/                       # Response DTOs
│   ├── Services/                   # Application services (cycle detection)
│   └── Validators/                 # FluentValidation validators
├── Infrastructure/LifeGames.Infrastructure/  # EF Core, SQL Server, repositories
└── Api/LifeGames.Api/              # Controllers, DTOs, middleware

life-games-app/                     # React + TypeScript + Vite + Tailwind
├── src/components/                 # React components (Board, Controls, Patterns)
├── src/api/                        # API client
└── src/types/                      # TypeScript types
```

Each backend layer has corresponding `.Tests` project.

## Key Technical Decisions

- **.NET 8.0** with C# 12, classic `.sln` format (not `.slnx`) for tooling compatibility
- **SQL Server + EF Core** for persistence (LocalDB for development)
- **Sparse storage**: Store only alive cell coordinates as JSON
- **Single Repository**: IBoardRepository handles both Board and BoardGeneration entities (aggregate pattern)
- **Navigation Properties**: Board has Generations collection, BoardGeneration has Board reference
- **Repository pattern**: No IQueryable leakage, repositories return domain objects
- **Query Optimization**: Handlers use navigation properties to reduce database calls (1 query instead of 2)
- **Board limits**: Cell coordinates bounded by int range, max 10,000 iterations for final state (configurable)
- **Cycle detection**: Detect static patterns and oscillators
- **Lazy persistence**: Store initial state always, computed generations on request
- **CORS**: Configured via Options pattern; allow specific origins per environment (never `AllowAnyOrigin` in production)

## Database Conventions

- **Table Names**: Use singular names (e.g., `Board` not `Boards`, `BoardGeneration` not `BoardGenerations`)
- **Primary Keys**: Use `Id` (Guid) for all entities
- **Foreign Keys**: Use `{Entity}Id` pattern (e.g., `BoardId`)
- **Indexes**: Add composite unique indexes on frequently queried columns

## API Conventions

- **Versioning**: Media type (`Accept: application/vnd.lifegames.v1+json`)
- **Errors**: Problem Details (RFC 7807)
- **Validation**: FluentValidation
- **Content negotiation**: JSON (default) and XML
- **HATEOAS**: Include links in responses

## Code Style

- Use MediatR for all use cases (all handlers in Handlers/ folder)
- Commands and queries are defined alongside their handlers in the same file
- Use navigation properties to access related entities (avoid redundant queries)
- Single repository pattern: IBoardRepository manages Board aggregate (including BoardGeneration)
- Configure via Options pattern with validation
- Propagate CancellationTokens through all layers
- Use Serilog for structured logging
- Follow standard C# naming conventions (PascalCase for public, _camelCase for private fields)

## Testing

- Unit tests for domain logic (Game of Life rules)
- Integration tests for API endpoints (using EF Core InMemory provider)
- Repository tests with in-memory database

## Frontend Architecture

- **Local Mode**: Client-side Game of Life computation (no API calls)
- **API Mode**: Server-side computation via fetch API
- **Components**: Board (SVG grid), Controls, Patterns
- **State Management**: React hooks (useState, useCallback, useEffect)
- **Styling**: Tailwind CSS v4 with @tailwindcss/vite plugin

## Don't Forget

### Backend
- Add health checks to new services
- Update Swagger documentation for new endpoints
- Handle cancellation in long-running computations
- Validate all user input with FluentValidation

### Frontend
- Add TypeScript types for all API responses
- Handle loading and error states for API calls
- Memoize expensive computations with useMemo/useCallback
- Test both local and API modes
