using FluentValidation;
using LifeGames.Application.Handlers;

namespace LifeGames.Application.Validators;

public class CreateBoardValidator : AbstractValidator<CreateBoardCommand>
{
    public CreateBoardValidator()
    {
        RuleFor(x => x.Cells)
            .NotNull()
            .WithMessage("Cells collection cannot be null");

        RuleFor(x => x.Name)
            .MaximumLength(200)
            .When(x => x.Name != null)
            .WithMessage("Board name cannot exceed 200 characters");
    }
}
