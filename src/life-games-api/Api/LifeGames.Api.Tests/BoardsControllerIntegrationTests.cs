using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using LifeGames.Api.DTOs;
using LifeGames.Application.DTOs;

namespace LifeGames.Api.Tests;

public class BoardsControllerIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public BoardsControllerIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task CreateBoard_ValidRequest_ReturnsCreated()
    {
        var request = new CreateBoardRequest(
            "Test Board",
            [new CellDto(0, 0), new CellDto(1, 0), new CellDto(2, 0)]
        );

        var response = await _client.PostAsJsonAsync("/api/boards", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var result = await response.Content.ReadFromJsonAsync<BoardResponseDto>();
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Board");
        result.GenerationNumber.Should().Be(0);
        result.Cells.Should().HaveCount(3);
        result.Links.Should().ContainKey("self");
        result.Links.Should().ContainKey("next");
        result.Links.Should().ContainKey("final");
    }

    [Fact]
    public async Task GetBoard_ExistingBoard_ReturnsOk()
    {
        var createRequest = new CreateBoardRequest(
            "Test Board",
            [new CellDto(0, 0), new CellDto(1, 0)]
        );
        var createResponse = await _client.PostAsJsonAsync("/api/boards", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<BoardResponseDto>();

        var response = await _client.GetAsync($"/api/boards/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BoardResponseDto>();
        result!.Id.Should().Be(created.Id);
    }

    [Fact]
    public async Task GetBoard_NonExistingBoard_ReturnsNotFound()
    {
        var response = await _client.GetAsync($"/api/boards/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetNextGeneration_ExistingBoard_ReturnsNextState()
    {
        // Create a blinker (horizontal line of 3 cells)
        var createRequest = new CreateBoardRequest(
            "Blinker",
            [new CellDto(0, 0), new CellDto(1, 0), new CellDto(2, 0)]
        );
        var createResponse = await _client.PostAsJsonAsync("/api/boards", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<BoardResponseDto>();

        var response = await _client.GetAsync($"/api/boards/{created!.Id}/next");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BoardResponseDto>();
        result!.GenerationNumber.Should().Be(1);
        // Blinker should now be vertical (3 cells in a column)
        result.Cells.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetGeneration_SpecificGeneration_ReturnsCorrectState()
    {
        var createRequest = new CreateBoardRequest(
            "Test Board",
            [new CellDto(0, 0), new CellDto(1, 0), new CellDto(2, 0)]
        );
        var createResponse = await _client.PostAsJsonAsync("/api/boards", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<BoardResponseDto>();

        var response = await _client.GetAsync($"/api/boards/{created!.Id}/generations/2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<BoardResponseDto>();
        result!.GenerationNumber.Should().Be(2);
    }

    [Fact]
    public async Task GetFinalState_StablePattern_DetectsStability()
    {
        // Create a block (2x2 square) which is a still life
        var createRequest = new CreateBoardRequest(
            "Block",
            [new CellDto(0, 0), new CellDto(1, 0), new CellDto(0, 1), new CellDto(1, 1)]
        );
        var createResponse = await _client.PostAsJsonAsync("/api/boards", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<BoardResponseDto>();

        var response = await _client.GetAsync($"/api/boards/{created!.Id}/final");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FinalStateResponseDto>();
        result!.IsCyclic.Should().BeFalse();
        result.Cells.Should().HaveCount(4);
    }

    [Fact]
    public async Task GetFinalState_Oscillator_DetectsCycle()
    {
        // Create a blinker which oscillates with period 2
        var createRequest = new CreateBoardRequest(
            "Blinker",
            [new CellDto(0, 0), new CellDto(1, 0), new CellDto(2, 0)]
        );
        var createResponse = await _client.PostAsJsonAsync("/api/boards", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<BoardResponseDto>();

        var response = await _client.GetAsync($"/api/boards/{created!.Id}/final");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<FinalStateResponseDto>();
        result!.IsCyclic.Should().BeTrue();
        result.CycleLength.Should().Be(2);
    }

    [Fact]
    public async Task HealthCheck_ReturnsHealthy()
    {
        var response = await _client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
