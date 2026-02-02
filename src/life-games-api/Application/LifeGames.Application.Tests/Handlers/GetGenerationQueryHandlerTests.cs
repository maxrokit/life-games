using FluentAssertions;
using LifeGames.Application.Handlers;
using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using LifeGames.Domain.ValueObjects;
using Moq;

namespace LifeGames.Application.Tests.Handlers;

public class GetGenerationQueryHandlerTests
{
    private readonly Mock<IBoardRepository> _mockRepository;
    private readonly GetGenerationQueryHandler _handler;

    public GetGenerationQueryHandlerTests()
    {
        _mockRepository = new Mock<IBoardRepository>();
        _handler = new GetGenerationQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_CachedGeneration_ReturnsCachedData()
    {
        var boardId = Guid.NewGuid();
        var board = Board.Create("Test Board", new HashSet<Cell> { new(0, 0) });
        var generation = BoardGeneration.Create(boardId, 5, new HashSet<Cell> { new(0, 0) });
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(generation, board);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        var query = new GetGenerationQuery(boardId, 5);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.GenerationNumber.Should().Be(5);
        result.Links.Should().ContainKey("previous");

        _mockRepository.Verify(x => x.AddGenerationAsync(It.IsAny<BoardGeneration>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_UncachedGeneration_ComputesAndStores()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell>
        {
            new(0, 0), new(1, 0), new(2, 0) // Blinker
        };
        var board = Board.Create("Test Board", cells);
        var initialGeneration = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(initialGeneration, board);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 2, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration?)null);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(initialGeneration);

        _mockRepository
            .Setup(x => x.AddGenerationAsync(It.IsAny<BoardGeneration>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration g, CancellationToken ct) =>
            {
                typeof(BoardGeneration).GetProperty("Board")!.SetValue(g, board);
                return g;
            });

        var query = new GetGenerationQuery(boardId, 2);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.GenerationNumber.Should().Be(2);
        result.Cells.Should().HaveCount(3); // Blinker at generation 2 is same as generation 0

        _mockRepository.Verify(x => x.AddGenerationAsync(
            It.Is<BoardGeneration>(g => g.GenerationNumber == 2),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_GenerationZero_IncludesNoPreviousLink()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell> { new(0, 0) };
        var board = Board.Create("Test Board", cells);
        var generation = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(generation, board);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        var query = new GetGenerationQuery(boardId, 0);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.Links.Should().NotContainKey("previous");
        result.Links.Should().ContainKey("self");
        result.Links.Should().ContainKey("board");
        result.Links.Should().ContainKey("next");
        result.Links.Should().ContainKey("final");
    }

    [Fact]
    public async Task Handle_NonExistingBoard_ReturnsNull()
    {
        var boardId = Guid.NewGuid();

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration?)null);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration?)null);

        var query = new GetGenerationQuery(boardId, 5);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_GenerationWithoutBoard_ReturnsNull()
    {
        var boardId = Guid.NewGuid();
        var generation = BoardGeneration.Create(boardId, 5, new HashSet<Cell>());

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        var query = new GetGenerationQuery(boardId, 5);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }
}
