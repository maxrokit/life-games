using LifeGames.Domain.ValueObjects;

namespace LifeGames.Domain.Entities;

public class BoardGeneration
{
    public Guid Id { get; private set; }
    public Guid BoardId { get; private set; }
    public int GenerationNumber { get; private set; }
    public HashSet<Cell> Cells { get; private set; } = [];
    public DateTime ComputedAt { get; private set; }

    // Navigation property
    public Board? Board { get; private set; }

    private BoardGeneration() { }

    public static BoardGeneration Create(Guid boardId, int generationNumber, IEnumerable<Cell> cells)
    {
        return new BoardGeneration
        {
            Id = Guid.NewGuid(),
            BoardId = boardId,
            GenerationNumber = generationNumber,
            Cells = [.. cells],
            ComputedAt = DateTime.UtcNow
        };
    }
}
