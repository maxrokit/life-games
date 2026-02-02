using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using LifeGames.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LifeGames.Infrastructure.Repositories;

public class BoardRepository(LifeGamesDbContext context) : IBoardRepository
{
    // Board operations
    public async Task<Board?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Boards
            .Include(b => b.Generations)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Board> AddAsync(Board board, CancellationToken cancellationToken = default)
    {
        context.Boards.Add(board);
        await context.SaveChangesAsync(cancellationToken);
        return board;
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var board = await context.Boards
            .Include(b => b.Generations)
            .FirstOrDefaultAsync(b => b.Id == id, cancellationToken);

        if (board != null)
        {
            context.Boards.Remove(board);
            await context.SaveChangesAsync(cancellationToken);
        }
    }

    // BoardGeneration operations
    public async Task<BoardGeneration?> GetGenerationAsync(
        Guid boardId,
        int generationNumber,
        CancellationToken cancellationToken = default)
    {
        return await context.BoardGenerations
            .Include(bg => bg.Board)
            .FirstOrDefaultAsync(
                bg => bg.BoardId == boardId && bg.GenerationNumber == generationNumber,
                cancellationToken);
    }

    public async Task<BoardGeneration?> GetLatestGenerationAsync(Guid boardId, CancellationToken cancellationToken = default)
    {
        return await context.BoardGenerations
            .Include(bg => bg.Board)
            .Where(bg => bg.BoardId == boardId)
            .OrderByDescending(bg => bg.GenerationNumber)
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<BoardGeneration> AddGenerationAsync(BoardGeneration generation, CancellationToken cancellationToken = default)
    {
        var boardExists = await context.Boards
            .AnyAsync(b => b.Id == generation.BoardId, cancellationToken);

        if (!boardExists)
            throw new InvalidOperationException($"Board with ID {generation.BoardId} not found");

        context.BoardGenerations.Add(generation);
        await context.SaveChangesAsync(cancellationToken);
        return generation;
    }
}
