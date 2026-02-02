namespace LifeGames.Domain.ValueObjects;

public readonly record struct Cell(int X, int Y)
{
    public IEnumerable<Cell> GetNeighbors()
    {
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                yield return new Cell(X + dx, Y + dy);
            }
        }
    }
}
