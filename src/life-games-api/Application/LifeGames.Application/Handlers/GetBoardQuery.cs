using LifeGames.Application.DTOs;
using LifeGames.Domain.Interfaces;
using MediatR;

namespace LifeGames.Application.Handlers;

public record GetBoardQuery(Guid BoardId) : IRequest<BoardResponseDto?>;

public class GetBoardQueryHandler(
    IBoardRepository boardRepository) : IRequestHandler<GetBoardQuery, BoardResponseDto?>
{
    public async Task<BoardResponseDto?> Handle(GetBoardQuery request, CancellationToken cancellationToken)
    {
        var generation = await boardRepository.GetGenerationAsync(request.BoardId, 0, cancellationToken);
        if (generation?.Board == null)
            return null;

        var cellDtos = generation.Cells.Select(c => new CellDto(c.X, c.Y)).ToList();

        return new BoardResponseDto(
            generation.Board.Id,
            generation.Board.Name,
            generation.Board.CreatedAt,
            generation.GenerationNumber,
            cellDtos,
            new Dictionary<string, LinkDto>
            {
                ["self"] = new LinkDto($"/api/boards/{generation.Board.Id}", "self"),
                ["next"] = new LinkDto($"/api/boards/{generation.Board.Id}/next", "next"),
                ["generation"] = new LinkDto($"/api/boards/{generation.Board.Id}/generations/{{n}}", "generation"),
                ["final"] = new LinkDto($"/api/boards/{generation.Board.Id}/final", "final")
            });
    }
}
