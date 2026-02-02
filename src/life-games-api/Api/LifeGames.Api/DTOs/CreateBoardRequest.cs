using LifeGames.Application.DTOs;

namespace LifeGames.Api.DTOs;

public record CreateBoardRequest(string? Name, IReadOnlyCollection<CellDto> Cells);
