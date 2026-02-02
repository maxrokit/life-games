using LifeGames.Application.DTOs;
using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using LifeGames.Domain.Services;
using MediatR;

namespace LifeGames.Application.Handlers;

public record GetGenerationQuery(Guid BoardId, int GenerationNumber) : IRequest<BoardResponseDto?>;

public class GetGenerationQueryHandler(
    IBoardRepository boardRepository) : IRequestHandler<GetGenerationQuery, BoardResponseDto?>
{
    public async Task<BoardResponseDto?> Handle(GetGenerationQuery request, CancellationToken cancellationToken)
    {
        // Check if generation is already cached
        var cachedGeneration = await boardRepository.GetGenerationAsync(
            request.BoardId, request.GenerationNumber, cancellationToken);

        BoardGeneration generation;
        if (cachedGeneration != null)
        {
            generation = cachedGeneration;
        }
        else
        {
            // Get initial generation and compute forward
            var initialGeneration = await boardRepository.GetGenerationAsync(
                request.BoardId, 0, cancellationToken);

            if (initialGeneration == null)
                return null;

            var computedCells = GameOfLifeEngine.ComputeGeneration(
                initialGeneration.Cells, request.GenerationNumber, cancellationToken);

            generation = BoardGeneration.Create(request.BoardId, request.GenerationNumber, computedCells);
            await boardRepository.AddGenerationAsync(generation, cancellationToken);
        }

        // Use navigation property from generation
        if (generation.Board == null)
            return null;

        var cellDtos = generation.Cells.Select(c => new CellDto(c.X, c.Y)).ToList();
        var links = new Dictionary<string, LinkDto>
        {
            ["self"] = new LinkDto($"/api/boards/{generation.Board.Id}/generations/{generation.GenerationNumber}", "self"),
            ["board"] = new LinkDto($"/api/boards/{generation.Board.Id}", "board"),
            ["next"] = new LinkDto($"/api/boards/{generation.Board.Id}/generations/{generation.GenerationNumber + 1}", "next"),
            ["final"] = new LinkDto($"/api/boards/{generation.Board.Id}/final", "final")
        };

        if (generation.GenerationNumber > 0)
        {
            links["previous"] = new LinkDto($"/api/boards/{generation.Board.Id}/generations/{generation.GenerationNumber - 1}", "previous");
        }

        return new BoardResponseDto(
            generation.Board.Id,
            generation.Board.Name,
            generation.Board.CreatedAt,
            generation.GenerationNumber,
            cellDtos,
            links);
    }
}
