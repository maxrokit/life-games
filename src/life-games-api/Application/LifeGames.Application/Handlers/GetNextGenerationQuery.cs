using LifeGames.Application.DTOs;
using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using LifeGames.Domain.Services;
using MediatR;

namespace LifeGames.Application.Handlers;

public record GetNextGenerationQuery(Guid BoardId) : IRequest<BoardResponseDto?>;

public class GetNextGenerationQueryHandler(
    IBoardRepository boardRepository) : IRequestHandler<GetNextGenerationQuery, BoardResponseDto?>
{
    public async Task<BoardResponseDto?> Handle(GetNextGenerationQuery request, CancellationToken cancellationToken)
    {
        var latestGeneration = await boardRepository.GetLatestGenerationAsync(request.BoardId, cancellationToken);
        if (latestGeneration?.Board == null)
            return null;

        int nextGenerationNumber = latestGeneration.GenerationNumber + 1;

        // Check if next generation is already cached
        var cachedGeneration = await boardRepository.GetGenerationAsync(
            request.BoardId, nextGenerationNumber, cancellationToken);

        BoardGeneration nextGeneration;
        if (cachedGeneration != null)
        {
            nextGeneration = cachedGeneration;
        }
        else
        {
            var nextCells = GameOfLifeEngine.ComputeNextGeneration(latestGeneration.Cells, cancellationToken);
            nextGeneration = BoardGeneration.Create(request.BoardId, nextGenerationNumber, nextCells);
            await boardRepository.AddGenerationAsync(nextGeneration, cancellationToken);
        }

        // Use Board from latest generation's navigation property
        var board = latestGeneration.Board;
        var cellDtos = nextGeneration.Cells.Select(c => new CellDto(c.X, c.Y)).ToList();

        return new BoardResponseDto(
            board.Id,
            board.Name,
            board.CreatedAt,
            nextGeneration.GenerationNumber,
            cellDtos,
            new Dictionary<string, LinkDto>
            {
                ["self"] = new LinkDto($"/api/boards/{board.Id}/generations/{nextGeneration.GenerationNumber}", "self"),
                ["board"] = new LinkDto($"/api/boards/{board.Id}", "board"),
                ["next"] = new LinkDto($"/api/boards/{board.Id}/next", "next"),
                ["previous"] = new LinkDto($"/api/boards/{board.Id}/generations/{nextGeneration.GenerationNumber - 1}", "previous"),
                ["final"] = new LinkDto($"/api/boards/{board.Id}/final", "final")
            });
    }
}
