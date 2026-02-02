using FluentAssertions;
using LifeGames.Application.Services;
using LifeGames.Domain.ValueObjects;

namespace LifeGames.Application.Tests;

public class CycleDetectionServiceTests
{
    private readonly CycleDetectionService _service = new();

    [Fact]
    public async Task DetectFinalState_EmptyBoard_ReturnsStableState()
    {
        var cells = new HashSet<Cell>();

        var result = await _service.DetectFinalStateAsync(cells, 1000);

        result.FinalState.Should().BeEmpty();
        result.IsCyclic.Should().BeFalse();
        result.CycleLength.Should().BeNull();
    }

    [Fact]
    public async Task DetectFinalState_Block_ReturnsStableState()
    {
        // Block is a still life (stable pattern)
        var cells = new HashSet<Cell>
        {
            new(0, 0), new(1, 0),
            new(0, 1), new(1, 1)
        };

        var result = await _service.DetectFinalStateAsync(cells, 1000);

        result.IsCyclic.Should().BeFalse();
        result.CycleLength.Should().BeNull();
        result.FinalState.Should().BeEquivalentTo(cells);
    }

    [Fact]
    public async Task DetectFinalState_Blinker_DetectsOscillator()
    {
        // Blinker is a period-2 oscillator
        var cells = new HashSet<Cell>
        {
            new(0, 0), new(1, 0), new(2, 0)
        };

        var result = await _service.DetectFinalStateAsync(cells, 1000);

        result.IsCyclic.Should().BeTrue();
        result.CycleLength.Should().Be(2);
    }

    [Fact]
    public async Task DetectFinalState_SingleCell_DiesAndStabilizes()
    {
        var cells = new HashSet<Cell> { new(0, 0) };

        var result = await _service.DetectFinalStateAsync(cells, 1000);

        result.FinalState.Should().BeEmpty();
        result.IsCyclic.Should().BeFalse();
    }

    [Fact]
    public async Task DetectFinalState_SupportsCancellation()
    {
        var cells = new HashSet<Cell>
        {
            new(0, 0), new(1, 0), new(2, 0)
        };
        var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () => await _service.DetectFinalStateAsync(cells, 1000, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task DetectFinalState_MaxIterationsReached_ReturnsCurrentState()
    {
        // Glider will not stabilize within 5 iterations
        var cells = new HashSet<Cell>
        {
            new(1, 0),
            new(2, 1),
            new(0, 2), new(1, 2), new(2, 2)
        };

        var result = await _service.DetectFinalStateAsync(cells, 5);

        result.FinalGeneration.Should().Be(5);
        result.IsCyclic.Should().BeFalse();
    }
}
