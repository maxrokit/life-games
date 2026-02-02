using LifeGames.Domain.ValueObjects;

namespace LifeGames.Domain.Services;

public static class GameOfLifeEngine
{
    public static HashSet<Cell> ComputeNextGeneration(IEnumerable<Cell> currentCells, CancellationToken cancellationToken = default)
    {
        var livingCells = currentCells.ToHashSet();
        var nextGeneration = new HashSet<Cell>();
        var cellsToCheck = new HashSet<Cell>();

        foreach (var cell in livingCells)
        {
            cancellationToken.ThrowIfCancellationRequested();

            cellsToCheck.Add(cell);
            foreach (var neighbor in cell.GetNeighbors())
            {
                cellsToCheck.Add(neighbor);
            }
        }

        foreach (var cell in cellsToCheck)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var neighborCount = CountLivingNeighbors(cell, livingCells);
            var isAlive = livingCells.Contains(cell);

            // Conway's rules:
            // 1. Any live cell with 2 or 3 neighbors survives
            // 2. Any dead cell with exactly 3 neighbors becomes alive
            // 3. All other cells die or stay dead
            if (isAlive && (neighborCount == 2 || neighborCount == 3))
            {
                nextGeneration.Add(cell);
            }
            else if (!isAlive && neighborCount == 3)
            {
                nextGeneration.Add(cell);
            }
        }

        return nextGeneration;
    }

    public static HashSet<Cell> ComputeGeneration(IEnumerable<Cell> initialCells, int targetGeneration, CancellationToken cancellationToken = default)
    {
        if (targetGeneration < 0)
            throw new ArgumentOutOfRangeException(nameof(targetGeneration), "Generation number must be non-negative");

        if (targetGeneration == 0)
            return initialCells.ToHashSet();

        var currentCells = initialCells.ToHashSet();

        for (int i = 0; i < targetGeneration; i++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            currentCells = ComputeNextGeneration(currentCells, cancellationToken);
        }

        return currentCells;
    }

    private static int CountLivingNeighbors(Cell cell, HashSet<Cell> livingCells)
    {
        int count = 0;
        foreach (var neighbor in cell.GetNeighbors())
        {
            if (livingCells.Contains(neighbor))
                count++;
        }
        return count;
    }
}
