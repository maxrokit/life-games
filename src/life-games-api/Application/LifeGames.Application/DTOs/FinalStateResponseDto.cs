namespace LifeGames.Application.DTOs;

public record FinalStateResponseDto(
    Guid BoardId,
    int FinalGenerationNumber,
    IReadOnlyCollection<CellDto> Cells,
    bool IsCyclic,
    int? CycleLength,
    int? CycleStartGeneration,
    bool ReachedMaxIterations,
    int MaxIterations,
    string Message,
    Dictionary<string, LinkDto> Links);
