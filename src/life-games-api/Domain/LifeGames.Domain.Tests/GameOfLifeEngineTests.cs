using FluentAssertions;
using LifeGames.Domain.Services;
using LifeGames.Domain.ValueObjects;

namespace LifeGames.Domain.Tests;

public class GameOfLifeEngineTests
{
    [Fact]
    public void EmptyBoard_RemainsEmpty()
    {
        var cells = Array.Empty<Cell>();

        var result = GameOfLifeEngine.ComputeNextGeneration(cells);

        result.Should().BeEmpty();
    }

    [Fact]
    public void SingleCell_Dies()
    {
        var cells = new[] { new Cell(0, 0) };

        var result = GameOfLifeEngine.ComputeNextGeneration(cells);

        result.Should().BeEmpty();
    }

    [Fact]
    public void Block_StillLife_RemainsStable()
    {
        // Block pattern (2x2 square) - stable
        var cells = new[]
        {
            new Cell(0, 0), new Cell(1, 0),
            new Cell(0, 1), new Cell(1, 1)
        };

        var result = GameOfLifeEngine.ComputeNextGeneration(cells);

        result.Should().BeEquivalentTo(cells);
    }

    [Fact]
    public void Beehive_StillLife_RemainsStable()
    {
        // Beehive pattern - stable
        var cells = new[]
        {
            new Cell(1, 0), new Cell(2, 0),
            new Cell(0, 1), new Cell(3, 1),
            new Cell(1, 2), new Cell(2, 2)
        };

        var result = GameOfLifeEngine.ComputeNextGeneration(cells);

        result.Should().BeEquivalentTo(cells);
    }

    [Fact]
    public void Blinker_Oscillator_OscillatesPeriod2()
    {
        // Horizontal blinker
        var horizontal = new[]
        {
            new Cell(0, 0), new Cell(1, 0), new Cell(2, 0)
        };

        // Vertical blinker (expected after 1 generation)
        var vertical = new[]
        {
            new Cell(1, -1), new Cell(1, 0), new Cell(1, 1)
        };

        var gen1 = GameOfLifeEngine.ComputeNextGeneration(horizontal);
        gen1.Should().BeEquivalentTo(vertical);

        var gen2 = GameOfLifeEngine.ComputeNextGeneration(gen1);
        gen2.Should().BeEquivalentTo(horizontal);
    }

    [Fact]
    public void Toad_Oscillator_OscillatesPeriod2()
    {
        // Toad pattern phase 1
        var phase1 = new[]
        {
            new Cell(1, 0), new Cell(2, 0), new Cell(3, 0),
            new Cell(0, 1), new Cell(1, 1), new Cell(2, 1)
        };

        var gen1 = GameOfLifeEngine.ComputeNextGeneration(phase1);
        var gen2 = GameOfLifeEngine.ComputeNextGeneration(gen1);

        gen2.Should().BeEquivalentTo(phase1);
    }

    [Fact]
    public void Glider_Spaceship_MovesCorrectly()
    {
        // Glider pattern
        var glider = new[]
        {
            new Cell(1, 0),
            new Cell(2, 1),
            new Cell(0, 2), new Cell(1, 2), new Cell(2, 2)
        };

        // After 4 generations, glider should move diagonally by (1, 1)
        var gen4 = GameOfLifeEngine.ComputeGeneration(glider, 4);

        var expectedGlider = new[]
        {
            new Cell(2, 1),
            new Cell(3, 2),
            new Cell(1, 3), new Cell(2, 3), new Cell(3, 3)
        };

        gen4.Should().BeEquivalentTo(expectedGlider);
    }

    [Fact]
    public void ComputeGeneration_Generation0_ReturnsInitialState()
    {
        var cells = new[] { new Cell(0, 0), new Cell(1, 0) };

        var result = GameOfLifeEngine.ComputeGeneration(cells, 0);

        result.Should().BeEquivalentTo(cells);
    }

    [Fact]
    public void ComputeGeneration_NegativeGeneration_ThrowsArgumentOutOfRangeException()
    {
        var cells = new[] { new Cell(0, 0) };

        var act = () => GameOfLifeEngine.ComputeGeneration(cells, -1);

        act.Should().Throw<ArgumentOutOfRangeException>();
    }

    [Fact]
    public void ComputeNextGeneration_SupportsCancellation()
    {
        var cells = new[] { new Cell(0, 0), new Cell(1, 0), new Cell(2, 0) };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => GameOfLifeEngine.ComputeNextGeneration(cells, cts.Token);

        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void ComputeGeneration_SupportsCancellation()
    {
        var cells = new[] { new Cell(0, 0), new Cell(1, 0), new Cell(2, 0) };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = () => GameOfLifeEngine.ComputeGeneration(cells, 10, cts.Token);

        act.Should().Throw<OperationCanceledException>();
    }

    [Fact]
    public void ThreeCellsInRow_BecomeThreeCellsInColumn()
    {
        // Rule verification: cell with exactly 3 neighbors becomes alive
        var cells = new[] { new Cell(0, 0), new Cell(1, 0), new Cell(2, 0) };

        var result = GameOfLifeEngine.ComputeNextGeneration(cells);

        // Middle cell survives (2 neighbors)
        // Top and bottom of middle cell are born (3 neighbors each)
        result.Should().Contain(new Cell(1, 0));
        result.Should().Contain(new Cell(1, -1));
        result.Should().Contain(new Cell(1, 1));
        result.Count.Should().Be(3);
    }

    [Fact]
    public void CellWith4Neighbors_Dies()
    {
        // Cross pattern with center cell having 4 neighbors
        var cells = new[]
        {
            new Cell(1, 0),
            new Cell(0, 1), new Cell(1, 1), new Cell(2, 1),
            new Cell(1, 2)
        };

        var result = GameOfLifeEngine.ComputeNextGeneration(cells);

        // Center cell (1,1) has 4 neighbors and should die
        result.Should().NotContain(new Cell(1, 1));
    }
}
