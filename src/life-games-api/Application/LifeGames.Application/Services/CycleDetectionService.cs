using LifeGames.Domain.Services;
using LifeGames.Domain.ValueObjects;

namespace LifeGames.Application.Services;

public class CycleDetectionService : ICycleDetectionService
{
    public Task<CycleDetectionResult> DetectFinalStateAsync(
        HashSet<Cell> initialCells,
        int maxIterations,
        CancellationToken cancellationToken = default)
    {
        return Task.Run(() => DetectFinalState(initialCells, maxIterations, cancellationToken), cancellationToken);
    }

    private static CycleDetectionResult DetectFinalState(
        HashSet<Cell> initialCells,
        int maxIterations,
        CancellationToken cancellationToken)
    {
        var stateHistory = new Dictionary<string, int>();
        var currentCells = initialCells;
        int generation = 0;

        string stateKey = GetStateKey(currentCells);
        stateHistory[stateKey] = generation;

        while (generation < maxIterations)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nextCells = GameOfLifeEngine.ComputeNextGeneration(currentCells, cancellationToken);
            generation++;

            stateKey = GetStateKey(nextCells);

            if (stateHistory.TryGetValue(stateKey, out int previousGeneration))
            {
                int cycleLength = generation - previousGeneration;

                if (cycleLength == 1 && nextCells.SetEquals(currentCells))
                {
                    // Stable state (still life)
                    return new CycleDetectionResult(
                        nextCells,
                        generation,
                        IsCyclic: false,
                        CycleLength: null,
                        CycleStartGeneration: null,
                        ReachedMaxIterations: false);
                }

                // Cyclic pattern (oscillator)
                return new CycleDetectionResult(
                    nextCells,
                    generation,
                    IsCyclic: true,
                    CycleLength: cycleLength,
                    CycleStartGeneration: previousGeneration,
                    ReachedMaxIterations: false);
            }

            // Empty board is stable
            if (nextCells.Count == 0)
            {
                return new CycleDetectionResult(
                    nextCells,
                    generation,
                    IsCyclic: false,
                    CycleLength: null,
                    CycleStartGeneration: null,
                    ReachedMaxIterations: false);
            }

            stateHistory[stateKey] = generation;
            currentCells = nextCells;
        }

        // Max iterations reached without finding cycle
        return new CycleDetectionResult(
            currentCells,
            generation,
            IsCyclic: false,
            CycleLength: null,
            CycleStartGeneration: null,
            ReachedMaxIterations: true);
    }

    private static string GetStateKey(HashSet<Cell> cells)
    {
        if (cells.Count == 0)
            return "empty";

        var sortedCells = cells.OrderBy(c => c.X).ThenBy(c => c.Y);
        return string.Join(";", sortedCells.Select(c => $"{c.X},{c.Y}"));
    }
}
