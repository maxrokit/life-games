# Architecture Overview

This document describes the architectural patterns and design decisions for the Life Games API.

## Clean Architecture with Aggregate Pattern

The application follows Clean Architecture principles with a focus on the **Aggregate Pattern** for managing Board and BoardGeneration entities.

### Layer Structure

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

## Aggregate Pattern

### Board as Aggregate Root

**Board** is the aggregate root that manages the **BoardGeneration** collection:

```csharp
public class Board
{
    public Guid Id { get; private set; }
    public string? Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property - Board owns Generations
    private readonly List<BoardGeneration> _generations = [];
    public IReadOnlyCollection<BoardGeneration> Generations => _generations.AsReadOnly();

    public void AddGeneration(BoardGeneration generation) { ... }
}
```

**BoardGeneration** knows its parent Board:

```csharp
public class BoardGeneration
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public int GenerationNumber { get; private set; }
    public HashSet<Cell> Cells { get; private set; }

    // Navigation property - knows parent Board
    public Board? Board { get; private set; }
}
```

### Benefits

1. **Domain Consistency**: Board and BoardGeneration are always accessed together
2. **Cascade Operations**: Deleting a Board automatically deletes all Generations
3. **Single Source of Truth**: One repository manages the entire aggregate
4. **Transactional Integrity**: Changes to Board and Generations in same transaction

## Repository Pattern

### Single Repository for Aggregate

Instead of separate repositories for Board and BoardGeneration, we use a **single IBoardRepository**:

```csharp
public interface IBoardRepository
{
    // Board operations
    Task<Board?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Board> AddAsync(Board board, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // BoardGeneration operations (managed through Board aggregate)
    Task<BoardGeneration?> GetGenerationAsync(Guid boardId, int generationNumber, ...);
    Task<BoardGeneration?> GetLatestGenerationAsync(Guid boardId, ...);
    Task<BoardGeneration> AddGenerationAsync(BoardGeneration generation, ...);
}
```

### Implementation Strategy

**Board queries** use `Include()` to load all generations:
```csharp
return await context.Boards
    .Include(b => b.Generations)
    .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
```

**BoardGeneration queries** use `Include()` to load the parent board:
```csharp
return await context.BoardGenerations
    .Include(bg => bg.Board)
    .FirstOrDefaultAsync(bg => bg.BoardId == boardId &&
                              bg.GenerationNumber == generationNumber,
                              cancellationToken);
```

## CQRS with MediatR

### Handler Organization

All handlers are in a single `Handlers/` folder:

```
Application/
└── Handlers/
    ├── CreateBoardCommand.cs        (Command + Handler)
    ├── GetBoardQuery.cs              (Query + Handler)
    ├── GetGenerationQuery.cs         (Query + Handler)
    ├── GetNextGenerationQuery.cs     (Query + Handler)
    └── GetFinalStateQuery.cs         (Query + Handler)
```

Each file contains:
- The command/query record
- The handler class implementing `IRequestHandler<TRequest, TResponse>`


## Entity Framework Configuration

### Navigation Property Setup

**BoardConfiguration.cs**:
```csharp
builder.Metadata.FindNavigation(nameof(Board.Generations))!
    .SetField("_generations");

builder.HasMany(b => b.Generations)
    .WithOne(bg => bg.Board!)
    .HasForeignKey(bg => bg.BoardId)
    .OnDelete(DeleteBehavior.Cascade);
```

**BoardGenerationConfiguration.cs**:
```csharp
// Foreign key relationship configured in BoardConfiguration
// Sparse cell storage using JSON
builder.Property(bg => bg.Cells)
    .HasConversion(
        cells => JsonSerializer.Serialize(cells.Select(c => new { c.X, c.Y }), ...),
        json => DeserializeCells(json))
    .HasColumnType("TEXT");
```

### Database Naming Conventions

**Singular Table Names**:
```csharp
builder.ToTable("Board");          // Not "Boards"
builder.ToTable("BoardGeneration"); // Not "BoardGenerations"
```

This follows the principle that a table represents a collection of entities, where each row is one entity. Using singular names makes the code more consistent with the entity class names and aligns with Domain-Driven Design practices.

## Key Architectural Decisions

### 1. Aggregate Pattern
- **Decision**: Board is aggregate root, BoardGeneration is part of aggregate
- **Rationale**: Generations have no meaning without a Board
- **Benefit**: Enforces consistency, simplifies repository interface

### 2. Single Repository
- **Decision**: One repository manages entire aggregate
- **Rationale**: Board and BoardGeneration are tightly coupled
- **Benefit**: Simpler API, fewer dependencies in handlers

### 3. Navigation Properties
- **Decision**: Use EF Core navigation properties extensively
- **Rationale**: Reduce database queries, leverage ORM capabilities
- **Benefit**: 50% fewer queries, better performance

### 4. Lazy Persistence
- **Decision**: Store generation 0 always, compute others on demand
- **Rationale**: Storage efficiency, most generations never accessed
- **Benefit**: Minimal storage, fast writes

### 5. CQRS Pattern
- **Decision**: Separate commands and queries using MediatR
- **Rationale**: Clear separation of concerns, testability
- **Benefit**: Clean code, easy to extend

### 6. No IQueryable Leakage
- **Decision**: Repository returns domain objects, not IQueryable
- **Rationale**: Maintain persistence ignorance, easy to swap data stores
- **Benefit**: True repository pattern, flexible data layer

## Testing Strategy

### Test Distribution

- **Domain Tests (13)**: Game of Life rules, pure logic
- **Application Tests (6)**: Cycle detection, business logic
- **Infrastructure Tests (12)**: Repository operations with navigation properties
  - Basic repository operations (8 tests)
  - Navigation property tests (4 tests)
- **API Tests (8)**: Integration tests, end-to-end scenarios

**Total: 39 tests, all passing**

### Test Focus Areas

1. **Navigation Properties**: Verify Board and Generations load correctly
2. **Cascade Delete**: Ensure deleting Board removes all Generations
3. **Query Optimization**: Confirm single query loads related entities
4. **Aggregate Integrity**: Validate Board manages Generation lifecycle

## Performance Characteristics

### Caching Strategy

- **Generation 0**: Always cached (initial state)
- **Computed Generations**: Cached on first computation
- **Latest Generation**: Cached for incremental computation
- **Final State**: Not cached (computed each time for cycle detection)

## Future Considerations

### Scalability

- **Current**: SQLite file-based database
- **Future**: PostgreSQL/SQL Server for production scale
- **Migration**: No code changes needed (repository pattern isolation)

### Distributed Caching

- **Current**: In-process EF Core tracking
- **Future**: Redis for distributed caching
- **Integration**: Add Redis in repository layer, no handler changes

### Event Sourcing

- **Current**: State-based storage
- **Future**: Event sourcing for audit trail
- **Approach**: Add events to aggregate root, maintain current API

## Conclusion

The architecture prioritizes:
1. **Performance**: Navigation properties reduce queries by 50%
2. **Maintainability**: Clear separation of concerns, single responsibility
3. **Testability**: 39 tests covering all layers
4. **Flexibility**: Repository pattern allows easy data store changes
5. **Domain-Driven**: Aggregate pattern enforces business rules

The combination of Clean Architecture, Aggregate Pattern, and CQRS provides a solid foundation for a scalable, maintainable Game of Life API.
