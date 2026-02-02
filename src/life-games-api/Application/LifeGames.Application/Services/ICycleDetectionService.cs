using LifeGames.Domain.ValueObjects;

namespace LifeGames.Application.Services;

public record CycleDetectionResult(
    HashSet<Cell> FinalState,
    int FinalGeneration,
    bool IsCyclic,
    int? CycleLength,
    int? CycleStartGeneration,
    bool ReachedMaxIterations);

public interface ICycleDetectionService
{
    Task<CycleDetectionResult> DetectFinalStateAsync(
        HashSet<Cell> initialCells,
        int maxIterations,
        CancellationToken cancellationToken = default);
}
