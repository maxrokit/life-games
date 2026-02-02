using FluentAssertions;
using LifeGames.Application.DTOs;
using LifeGames.Application.Handlers;
using LifeGames.Domain.Entities;
using LifeGames.Domain.Interfaces;
using Moq;

namespace LifeGames.Application.Tests.Handlers;

public class CreateBoardCommandHandlerTests
{
    private readonly Mock<IBoardRepository> _mockRepository;
    private readonly CreateBoardCommandHandler _handler;

    public CreateBoardCommandHandlerTests()
    {
        _mockRepository = new Mock<IBoardRepository>();
        _handler = new CreateBoardCommandHandler(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_ValidCommand_CreatesBoard()
    {
        var cells = new List<CellDto> { new(0, 0), new(1, 0), new(2, 0) };
        var command = new CreateBoardCommand("Test Board", cells);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Board>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board b, CancellationToken ct) => b);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().Be("Test Board");
        result.GenerationNumber.Should().Be(0);
        result.Cells.Should().HaveCount(3);
        result.Links.Should().ContainKey("self");
        result.Links.Should().ContainKey("next");
        result.Links.Should().ContainKey("generation");
        result.Links.Should().ContainKey("final");

        _mockRepository.Verify(x => x.AddAsync(
            It.Is<Board>(b => b.Name == "Test Board" && b.Generations.Count > 0),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_NullName_CreatesBoard()
    {
        var cells = new List<CellDto> { new(0, 0) };
        var command = new CreateBoardCommand(null, cells);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Board>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board b, CancellationToken ct) => b);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Name.Should().BeNull();
    }

    [Fact]
    public async Task Handle_EmptyCells_CreatesEmptyBoard()
    {
        var command = new CreateBoardCommand("Empty Board", []);

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Board>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Board b, CancellationToken ct) => b);

        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.Cells.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_PassesCancellationToken()
    {
        var cells = new List<CellDto> { new(0, 0) };
        var command = new CreateBoardCommand("Test", cells);
        var cts = new CancellationTokenSource();
        cts.Cancel();

        _mockRepository
            .Setup(x => x.AddAsync(It.IsAny<Board>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new OperationCanceledException());

        var act = async () => await _handler.Handle(command, cts.Token);

        await act.Should().ThrowAsync<OperationCanceledException>();
    }
}
