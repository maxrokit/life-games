using LifeGames.Domain.ValueObjects;

namespace LifeGames.Domain.Entities;

public class Board
{
    public Guid Id { get; private set; }
    public string? Name { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation property
    private readonly List<BoardGeneration> _generations = [];
    public IReadOnlyCollection<BoardGeneration> Generations => _generations.AsReadOnly();

    private Board() { }

    public static Board Create(string? name, IEnumerable<Cell> cells)
    {
        var board = Create(name);
        var generation = BoardGeneration.Create(board.Id, 0, cells);
        board.AddGeneration(generation);
        return board;
    }

    public void AddGeneration(BoardGeneration generation)
    {
        if (generation.BoardId != Id)
            throw new InvalidOperationException("Generation does not belong to this board");

        _generations.Add(generation);
    }
    private static Board Create(string? name = null)
    {
        return new Board
        {
            Id = Guid.NewGuid(),
            Name = name,
            CreatedAt = DateTime.UtcNow
        };
    }
}
