using LifeGames.Application.DTOs;
using LifeGames.Application.Options;
using LifeGames.Application.Services;
using LifeGames.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Options;

namespace LifeGames.Application.Handlers;

public record GetFinalStateQuery(Guid BoardId) : IRequest<FinalStateResponseDto?>;

public class GetFinalStateQueryHandler(
    IBoardRepository boardRepository,
    ICycleDetectionService cycleDetectionService,
    IOptions<BoardOptions> boardOptions) : IRequestHandler<GetFinalStateQuery, FinalStateResponseDto?>
{
    public async Task<FinalStateResponseDto?> Handle(GetFinalStateQuery request, CancellationToken cancellationToken)
    {
        var initialGeneration = await boardRepository.GetGenerationAsync(
            request.BoardId, 0, cancellationToken);

        if (initialGeneration?.Board == null)
            return null;

        var result = await cycleDetectionService.DetectFinalStateAsync(
            initialGeneration.Cells,
            boardOptions.Value.MaxIterationsForFinalState,
            cancellationToken);

        var cellDtos = result.FinalState.Select(c => new CellDto(c.X, c.Y)).ToList();
        var message = GetResultMessage(result, boardOptions.Value.MaxIterationsForFinalState);

        return new FinalStateResponseDto(
            initialGeneration.Board.Id,
            result.FinalGeneration,
            cellDtos,
            result.IsCyclic,
            result.CycleLength,
            result.CycleStartGeneration,
            result.ReachedMaxIterations,
            boardOptions.Value.MaxIterationsForFinalState,
            message,
            new Dictionary<string, LinkDto>
            {
                ["self"] = new LinkDto($"/api/boards/{initialGeneration.Board.Id}/final", "self"),
                ["board"] = new LinkDto($"/api/boards/{initialGeneration.Board.Id}", "board"),
                ["generation"] = new LinkDto($"/api/boards/{initialGeneration.Board.Id}/generations/{result.FinalGeneration}", "generation")
            });
    }

    private static string GetResultMessage(CycleDetectionResult result, int maxIterations)
    {
        if (result.ReachedMaxIterations)
        {
            return $"Board did not reach a stable state within {maxIterations:N0} iterations. " +
                   "The pattern may be chaotic or have a very long cycle.";
        }

        if (result.IsCyclic)
        {
            return result.CycleLength == 1
                ? $"Board reached a stable state (still life) at generation {result.FinalGeneration}."
                : $"Board entered a cycle of length {result.CycleLength} starting at generation {result.CycleStartGeneration}.";
        }

        return result.FinalState.Count == 0
            ? $"Board died out (all cells dead) at generation {result.FinalGeneration}."
            : $"Board reached a stable state at generation {result.FinalGeneration}.";
    }
}
