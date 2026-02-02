using FluentAssertions;
using LifeGames.Application.Handlers;
using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using LifeGames.Domain.ValueObjects;
using Moq;

namespace LifeGames.Application.Tests.Handlers;

public class GetNextGenerationQueryHandlerTests
{
    private readonly Mock<IBoardRepository> _mockRepository;
    private readonly GetNextGenerationQueryHandler _handler;

    public GetNextGenerationQueryHandlerTests()
    {
        _mockRepository = new Mock<IBoardRepository>();
        _handler = new GetNextGenerationQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_CachedNextGeneration_ReturnsCachedData()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell> { new(0, 0) };
        var board = Board.Create("Test Board", cells);
        var latestGeneration = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(latestGeneration, board);

        var nextGeneration = BoardGeneration.Create(boardId, 1, new HashSet<Cell> { new(1, 1) });

        _mockRepository
            .Setup(x => x.GetLatestGenerationAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestGeneration);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nextGeneration);

        var query = new GetNextGenerationQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.GenerationNumber.Should().Be(1);

        _mockRepository.Verify(x => x.AddGenerationAsync(It.IsAny<BoardGeneration>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UncachedNextGeneration_ComputesAndStores()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell>
        {
            new(0, 0), new(1, 0), new(2, 0) // Blinker horizontal
        };
        var board = Board.Create("Test Board", cells);
        var latestGeneration = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(latestGeneration, board);

        _mockRepository
            .Setup(x => x.GetLatestGenerationAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestGeneration);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 1, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration?)null);

        _mockRepository
            .Setup(x => x.AddGenerationAsync(It.IsAny<BoardGeneration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration g, CancellationToken ct) =>
            {
                typeof(BoardGeneration).GetProperty("Board")!.SetValue(g, board);
                return g;
            });

        var query = new GetNextGenerationQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.GenerationNumber.Should().Be(1);
        result.Cells.Should().HaveCount(3); // Blinker becomes vertical

        _mockRepository.Verify(x => x.AddGenerationAsync(
            It.Is<BoardGeneration>(g => g.GenerationNumber == 1 && g.BoardId == boardId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_IncludesAllRequiredLinks()
    {
        var boardId = Guid.NewGuid();
        var board = Board.Create("Test Board", new HashSet<Cell> { new(0, 0) });
        var latestGeneration = BoardGeneration.Create(boardId, 5, new HashSet<Cell>());
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(latestGeneration, board);

        var nextGeneration = BoardGeneration.Create(boardId, 6, new HashSet<Cell>());

        _mockRepository
            .Setup(x => x.GetLatestGenerationAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(latestGeneration);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 6, It.IsAny<CancellationToken>()))
            .ReturnsAsync(nextGeneration);

        var query = new GetNextGenerationQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Links.Should().ContainKey("self");
        result.Links.Should().ContainKey("board");
        result.Links.Should().ContainKey("next");
        result.Links.Should().ContainKey("previous");
        result.Links.Should().ContainKey("final");
    }

    [Fact]
    public async Task Handle_NonExistingBoard_ReturnsNull()
    {
        var boardId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetLatestGenerationAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration?)null);

        var query = new GetNextGenerationQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_LatestGenerationWithoutBoard_ReturnsNull()
    {
        var boardId = Guid.NewGuid();
        var generation = BoardGeneration.Create(boardId, 0, new HashSet<Cell>());

        _mockRepository
            .Setup(x => x.GetLatestGenerationAsync(boardId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        var query = new GetNextGenerationQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
