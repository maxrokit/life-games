using FluentAssertions;
using LifeGames.Application.Handlers;
using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using LifeGames.Domain.ValueObjects;
using Moq;

namespace LifeGames.Application.Tests.Handlers;

public class GetBoardQueryHandlerTests
{
    private readonly Mock<IBoardRepository> _mockRepository;
    private readonly GetBoardQueryHandler _handler;

    public GetBoardQueryHandlerTests()
    {
        _mockRepository = new Mock<IBoardRepository>();
        _handler = new GetBoardQueryHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ExistingBoard_ReturnsBoard()
    {
        var boardId = Guid.NewGuid();
        var cells = new HashSet<Cell> { new(0, 0), new(1, 0) };
        var board = Board.Create("Test Board", cells);
        var generation = BoardGeneration.Create(boardId, 0, cells);
        typeof(BoardGeneration).GetProperty("Board")!.SetValue(generation, board);

        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync(generation);

        var query = new GetBoardQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().NotBeNull();
        result!.GenerationNumber.Should().Be(0);
        result.Cells.Should().HaveCount(2);
        result.Links.Should().ContainKey("self");
        result.Links.Should().ContainKey("next");
        result.Links.Should().ContainKey("generation");
        result.Links.Should().ContainKey("final");
    }

    [Fact]
    public async Task Handle_NonExistingBoard_ReturnsNull()
    {
        var boardId = Guid.NewGuid();
        _mockRepository
            .Setup(x => x.GetGenerationAsync(boardId, 0, It.IsAny<CancellationToken>()))
            .ReturnsAsync((BoardGeneration?)null);

        var query = new GetBoardQuery(boardId);
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

        var query = new GetBoardQuery(boardId);
        var result = await _handler.Handle(query, CancellationToken.None);

        result.Should().BeNull();
    }

    [Fact]
    public async Task Handle_PassesCancellationToken()
    {
        var boardId = Guid.NewGuid();
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepository
            .Setup(x => x.GetGenerationAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var query = new GetBoardQuery(boardId);
        var act = async () => await _handler.Handle(query, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
