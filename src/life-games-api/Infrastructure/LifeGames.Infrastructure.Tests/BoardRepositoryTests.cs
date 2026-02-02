using FluentAssertions;
using LifeGames.Domain.Entities;
using LifeGames.Domain.ValueObjects;
using LifeGames.Infrastructure.Data;
using LifeGames.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace LifeGames.Infrastructure.Tests;

public class BoardRepositoryTests : IDisposable
{
    private readonly LifeGamesDbContext _context;
    private readonly BoardRepository _repository;

    public BoardRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<LifeGamesDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new LifeGamesDbContext(options);
        _repository = new BoardRepository(_context);
    }

    [Fact]
    public async Task AddAsync_ShouldAddBoard()
    {
        var cells = new[] { new Cell(0, 0), new Cell(1, 0) };
        var board = Board.Create("Test Board", cells);

        var result = await _repository.AddAsync(board);

        result.Should().NotBeNull();
        result.Id.Should().Be(board.Id);
        result.Name.Should().Be("Test Board");
    }

    [Fact]
    public async Task GetByIdAsync_ExistingBoard_ShouldReturnBoard()
    {
        var cells = new[] { new Cell(0, 0) };
        var board = Board.Create("Test Board", cells);
        await _repository.AddAsync(board);

        var result = await _repository.GetByIdAsync(board.Id);

        result.Should().NotBeNull();
        result!.Id.Should().Be(board.Id);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingBoard_ShouldReturnNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingBoard_ShouldRemoveBoard()
    {
        var cells = new[] { new Cell(0, 0) };
        var board = Board.Create("Test Board", cells);
        await _repository.AddAsync(board);

        await _repository.DeleteAsync(board.Id);

        var result = await _repository.GetByIdAsync(board.Id);
        result.Should().BeNull();
    }

    // BoardGeneration tests
    [Fact]
    public async Task AddGenerationAsync_ShouldAddGeneration()
    {
        var cells = new[] { new Cell(0, 0), new Cell(1, 0) };
        var board = Board.Create("Test Board", cells);
        await _repository.AddAsync(board);

        var generation = BoardGeneration.Create(board.Id, 1, cells);

        var result = await _repository.AddGenerationAsync(generation);

        result.Should().NotBeNull();
        result.BoardId.Should().Be(board.Id);
        result.GenerationNumber.Should().Be(1);
        result.Cells.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetGenerationAsync_ExistingGeneration_ShouldReturn()
    {
        var cells = new[] { new Cell(0, 0), new Cell(1, 0) };
        var board = Board.Create("Test Board", cells);
        await _repository.AddAsync(board);

        var result = await _repository.GetGenerationAsync(board.Id, 0);

        result.Should().NotBeNull();
        result!.Cells.Should().BeEquivalentTo(cells);
    }

    [Fact]
    public async Task GetGenerationAsync_NonExisting_ShouldReturnNull()
    {
        var boardId = Guid.NewGuid();

        var result = await _repository.GetGenerationAsync(boardId, 99);

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetLatestGenerationAsync_ShouldReturnHighestGeneration()
    {
        var cells = new[] { new Cell(0, 0) };
        var board = Board.Create("Test Board", cells);
        await _repository.AddAsync(board);

        await _repository.AddGenerationAsync(BoardGeneration.Create(board.Id, 1, cells));
        await _repository.AddGenerationAsync(BoardGeneration.Create(board.Id, 2, cells));

        var result = await _repository.GetLatestGenerationAsync(board.Id);

        result.Should().NotBeNull();
        result!.GenerationNumber.Should().Be(2);
    }

    // Navigation Property tests
    [Fact]
    public async Task GetGenerationAsync_ShouldIncludeBoardNavigationProperty()
    {
        var cells = new[] { new Cell(0, 0), new Cell(1, 0) };
        var board = Board.Create("Navigation Test Board", cells);
        await _repository.AddAsync(board);

        var result = await _repository.GetGenerationAsync(board.Id, 0);

        result.Should().NotBeNull();
        result!.Board.Should().NotBeNull();
        result.Board!.Id.Should().Be(board.Id);
        result.Board.Name.Should().Be("Navigation Test Board");
    }

    [Fact]
    public async Task GetLatestGenerationAsync_ShouldIncludeBoardNavigationProperty()
    {
        var cells = new[] { new Cell(0, 0) };
        var board = Board.Create("Latest Gen Test", cells);
        await _repository.AddAsync(board);

        await _repository.AddGenerationAsync(BoardGeneration.Create(board.Id, 5, cells));

        var result = await _repository.GetLatestGenerationAsync(board.Id);

        result.Should().NotBeNull();
        result!.Board.Should().NotBeNull();
        result.Board!.Id.Should().Be(board.Id);
        result.Board.Name.Should().Be("Latest Gen Test");
        result.GenerationNumber.Should().Be(5);
    }

    [Fact]
    public async Task GetByIdAsync_ShouldIncludeGenerationsCollection()
    {
        var cells = new[] { new Cell(0, 0) };
        var board = Board.Create("Board with Generations", cells);
        await _repository.AddAsync(board);

        await _repository.AddGenerationAsync(BoardGeneration.Create(board.Id, 1, cells));
        await _repository.AddGenerationAsync(BoardGeneration.Create(board.Id, 2, cells));

        var result = await _repository.GetByIdAsync(board.Id);

        result.Should().NotBeNull();
        result!.Generations.Should().HaveCount(3);
        result.Generations.Should().Contain(g => g.GenerationNumber == 0);
        result.Generations.Should().Contain(g => g.GenerationNumber == 1);
        result.Generations.Should().Contain(g => g.GenerationNumber == 2);
    }

    [Fact]
    public async Task DeleteAsync_ShouldCascadeDeleteGenerations()
    {
        var cells = new[] { new Cell(0, 0) };
        var board = Board.Create("Board to Delete", cells);
        await _repository.AddAsync(board);

        await _repository.AddGenerationAsync(BoardGeneration.Create(board.Id, 1, cells));

        await _repository.DeleteAsync(board.Id);

        var deletedBoard = await _repository.GetByIdAsync(board.Id);
        var deletedGeneration = await _repository.GetGenerationAsync(board.Id, 0);

        deletedBoard.Should().BeNull();
        deletedGeneration.Should().BeNull();
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
