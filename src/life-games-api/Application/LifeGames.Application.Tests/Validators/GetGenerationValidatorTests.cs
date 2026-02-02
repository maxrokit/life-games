using FluentAssertions;
using FluentValidation.TestHelper;
using LifeGames.Application.Handlers;
using LifeGames.Application.Validators;

namespace LifeGames.Application.Tests.Validators;

public class GetGenerationValidatorTests
{
    private readonly GetGenerationValidator _validator;

    public GetGenerationValidatorTests()
    {
        _validator = new GetGenerationValidator();
    }

    [Fact]
    public void Validate_ValidQuery_PassesValidation()
    {
        var query = new GetGenerationQuery(Guid.NewGuid(), 5);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_GenerationNumberZero_PassesValidation()
    {
        var query = new GetGenerationQuery(Guid.NewGuid(), 0);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_NegativeGenerationNumber_FailsValidation()
    {
        var query = new GetGenerationQuery(Guid.NewGuid(), -1);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.GenerationNumber)
            .WithErrorMessage("Generation number must be non-negative");
    }

    [Fact]
    public void Validate_EmptyBoardId_FailsValidation()
    {
        var query = new GetGenerationQuery(Guid.Empty, 5);

        var result = _validator.TestValidate(query);

        result.ShouldHaveValidationErrorFor(x => x.BoardId)
            .WithErrorMessage("Board ID is required");
    }

    [Fact]
    public void Validate_LargeGenerationNumber_PassesValidation()
    {
        var query = new GetGenerationQuery(Guid.NewGuid(), 1000000);

        var result = _validator.TestValidate(query);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
