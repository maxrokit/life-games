using LifeGames.Application.DTOs;
using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using LifeGames.Domain.ValueObjects;
using MediatR;

namespace LifeGames.Application.Handlers;

public record CreateBoardCommand(string? Name, IReadOnlyCollection<CellDto> Cells) : IRequest<BoardResponseDto>;

public class CreateBoardCommandHandler(
    IBoardRepository boardRepository) : IRequestHandler<CreateBoardCommand, BoardResponseDto>
{
    public async Task<BoardResponseDto> Handle(CreateBoardCommand request, CancellationToken cancellationToken)
    {
        var cells = request.Cells.Select(c => new Cell(c.X, c.Y)).ToHashSet();

        var board = Board.Create(request.Name, cells);
        await boardRepository.AddAsync(board, cancellationToken);

        var cellDtos = cells.Select(c => new CellDto(c.X, c.Y)).ToList();

        return new BoardResponseDto(
            board.Id,
            board.Name,
            board.CreatedAt,
            0,
            cellDtos,
            new Dictionary<string, LinkDto>
            {
                ["self"] = new LinkDto($"/api/boards/{board.Id}", "self"),
                ["next"] = new LinkDto($"/api/boards/{board.Id}/next", "next"),
                ["generation"] = new LinkDto($"/api/boards/{board.Id}/generations/{{n}}", "generation"),
                ["final"] = new LinkDto($"/api/boards/{board.Id}/final", "final")
            });
    }
}
