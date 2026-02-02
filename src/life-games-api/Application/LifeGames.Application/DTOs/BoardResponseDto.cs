namespace LifeGames.Application.DTOs;

public record BoardResponseDto(
    Guid Id,
    string? Name,
    DateTime CreatedAt,
    int GenerationNumber,
    IReadOnlyCollection<CellDto> Cells,
    Dictionary<string, LinkDto> Links);
