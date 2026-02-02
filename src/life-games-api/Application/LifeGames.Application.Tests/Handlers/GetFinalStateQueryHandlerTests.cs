using FluentAssertions;
using LifeGames.Application.Handlers;
using LifeGames.Application.Options;
using LifeGames.Application.Services;
using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using LifeGames.Domain.ValueObjects;
using Moq;

namespace LifeGames.Application.Tests.Handlers;

public class GetFinalStateQueryHandlerTests
{
    private readonly Mock<IBoardRepository> _mockRepository;
    private readonly Mock<ICycleDetectionService> _mockCycleService;
    private readonly Microsoft.Extensions.Options.IOptions<BoardOptions> _options;
    private readonly GetFinalStateQueryHandler _handler;

    public GetFinalStateQueryHandlerTests()
    {
        _mockRepository = new Mock<IBoardRepository>();
        _mockCycleService = new Mock<ICycleDetectionService>();
        _options = Microsoft.Extensions.Options.Options.Create(new BoardOptions { MaxIterationsForFinalState = 10000 });
        _handler = new GetFinalStateQueryHandler(_mockRepository.Object, _mockCycleService.Object, _options);
    }

    [Fact]
    public async Task Handle_StablePattern_ReturnsStableState()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell>
        {
            new(0, 0), new(1, 0), new(0, 1), new(1, 1)
        };
        var board = Board.Create("Block", cells);
        var initialGeneration = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(initialGeneration, board);

        var cycleResult = new CycleDetectionResult(
            FinalGeneration: 0,
            FinalState: cells,
            IsCyclic: false,
            CycleLength: null,
            CycleStartGeneration: null,
            ReachedMaxIterations: false
        );

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initialGeneration);

        _mockCycleService
            .Setup(x => x.DetectFinalStateAsync(cells, 10000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var query = new GetFinalStateQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsCyclic.Should().BeFalse();
        result.Cells.Should().HaveCount(4);
        result.Message.Should().Contain("stable state");
        result.Links.Should().ContainKey("self");
        result.Links.Should().ContainKey("board");
        result.Links.Should().ContainKey("generation");
    }

    [Fact]
    public async Task Handle_Oscillator_DetectsCycle()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell>
        {
            new(0, 0), new(1, 0), new(2, 0)
        };
        var board = Board.Create("Blinker", cells);
        var initialGeneration = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(initialGeneration, board);

        var cycleResult = new CycleDetectionResult(
            FinalGeneration: 2,
            FinalState: cells,
            IsCyclic: true,
            CycleLength: 2,
            CycleStartGeneration: 0,
            ReachedMaxIterations: false
        );

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initialGeneration);

        _mockCycleService
            .Setup(x => x.DetectFinalStateAsync(cells, 10000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var query = new GetFinalStateQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.IsCyclic.Should().BeTrue();
        result.CycleLength.Should().Be(2);
        result.CycleStartGeneration.Should().Be(0);
        result.Message.Should().Contain("cycle of length 2");
    }

    [Fact]
    public async Task Handle_StillLife_ReturnsAppropriateMessage()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell> { new(0, 0) };
        var board = Board.Create("Block", cells);
        var initialGeneration = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(initialGeneration, board);

        var cycleResult = new CycleDetectionResult(
            FinalGeneration: 1,
            FinalState: cells,
            IsCyclic: true,
            CycleLength: 1,
            CycleStartGeneration: 1,
            ReachedMaxIterations: false
        );

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initialGeneration);

        _mockCycleService
            .Setup(x => x.DetectFinalStateAsync(cells, 10000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var query = new GetFinalStateQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Message.Should().Contain("stable state (still life)");
    }

    [Fact]
    public async Task Handle_DiedOut_ReturnsAppropriateMessage()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell> { new(0, 0) };
        var board = Board.Create("Single Cell", cells);
        var initialGeneration = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(initialGeneration, board);

        var cycleResult = new CycleDetectionResult(
            FinalGeneration: 1,
            FinalState: new HashSet<Cell>(),
            IsCyclic: false,
            CycleLength: null,
            CycleStartGeneration: null,
            ReachedMaxIterations: false
        );

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initialGeneration);

        _mockCycleService
            .Setup(x => x.DetectFinalStateAsync(cells, 10000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var query = new GetFinalStateQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Message.Should().Contain("died out");
    }

    [Fact]
    public async Task Handle_MaxIterationsReached_ReturnsAppropriateMessage()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell> { new(0, 0) };
        var board = Board.Create("Chaotic", cells);
        var initialGeneration = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(initialGeneration, board);

        var cycleResult = new CycleDetectionResult(
            FinalGeneration: 10000,
            FinalState: new HashSet<Cell> { new(5, 5) },
            IsCyclic: false,
            CycleLength: null,
            CycleStartGeneration: null,
            ReachedMaxIterations: true
        );

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initialGeneration);

        _mockCycleService
            .Setup(x => x.DetectFinalStateAsync(cells, 10000, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cycleResult);

        var query = new GetFinalStateQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.ReachedMaxIterations.Should().BeTrue();
        result.Message.Should().Contain("did not reach a stable state");
        result.Message.Should().Contain("10,000 iterations");
    }

    [Fact]
    public async Task Handle_NonExistingBoard_ReturnsNull()
    {
        var boardId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration?)null);

        var query = new GetFinalStateQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_BoardWithoutNavigationProperty_ReturnsNull()
    {
        var boardId = Guid.NewGuid();
        var generation = BoardGeneration.Create(boardId, 0, new HashSet<Cell>());

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        var query = new GetFinalStateQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
