using Asp.Versioning;
using LifeGames.Api.DTOs;
using LifeGames.Application.DTOs;
using LifeGames.Application.Handlers;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace LifeGames.Api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/[controller]")]
[Produces("application/vnd.lifegames.v1+json", "application/vnd.lifegames.v1+xml", "application/json", "application/xml")]
public class BoardsController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// Creates a new board with the specified initial state.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BoardResponseDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBoard(
        [FromBody] CreateBoardRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateBoardCommand(request.Name, request.Cells);
        var result = await mediator.Send(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetBoard),
            new { id = result.Id },
            result);
    }

    /// <summary>
    /// Gets a board by its ID (returns generation 0).
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BoardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBoard(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetBoardQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets the next generation of the board.
    /// </summary>
    [HttpGet("{id:guid}/next")]
    [ProducesResponseType(typeof(BoardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetNextGeneration(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetNextGenerationQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets a specific generation of the board.
    /// </summary>
    [HttpGet("{id:guid}/generations/{generationNumber:int}")]
    [ProducesResponseType(typeof(BoardResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetGeneration(
        Guid id,
        int generationNumber,
        CancellationToken cancellationToken)
    {
        var query = new GetGenerationQuery(id, generationNumber);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Gets the final/stable state of the board (detects cycles or still lifes).
    /// </summary>
    [HttpGet("{id:guid}/final")]
    [ProducesResponseType(typeof(FinalStateResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetFinalState(Guid id, CancellationToken cancellationToken)
    {
        var query = new GetFinalStateQuery(id);
        var result = await mediator.Send(query, cancellationToken);

        if (result == null)
            return NotFound();

        return Ok(result);
    }
}
