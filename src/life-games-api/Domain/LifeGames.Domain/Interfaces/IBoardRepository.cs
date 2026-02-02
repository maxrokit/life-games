using LifeGames.Domain.Entities;

namespace LifeGames.Domain.Interfaces;

public interface IBoardRepository
{
    // Board operations
    Task<Board?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Board> AddAsync(Board board, CancellationToken cancellationToken = default);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    // BoardGeneration operations
    Task<BoardGeneration?> GetGenerationAsync(Guid boardId, int generationNumber, CancellationToken cancellationToken = default);
    Task<BoardGeneration?> GetLatestGenerationAsync(Guid boardId, CancellationToken cancellationToken = default);
    Task<BoardGeneration> AddGenerationAsync(BoardGeneration generation, CancellationToken cancellationToken = default);
}
