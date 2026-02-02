using FluentValidation;
using LifeGames.Application.Handlers;

namespace LifeGames.Application.Validators;

public class GetGenerationValidator : AbstractValidator<GetGenerationQuery>
{
    public GetGenerationValidator()
    {
        RuleFor(x => x.GenerationNumber)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Generation number must be non-negative");

        RuleFor(x => x.BoardId)
            .NotEmpty()
            .WithMessage("Board ID is required");
    }
}
